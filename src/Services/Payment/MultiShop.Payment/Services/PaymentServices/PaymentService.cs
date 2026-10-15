using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Contracts.Payments;
using MultiShop.Payment.Context;
using MultiShop.Payment.Dtos.PaymentDtos;
using MultiShop.Payment.Entities;
using MultiShop.Payment.Services.GarantiPosServices;

namespace MultiShop.Payment.Services.PaymentServices
{
    /// <summary>
    /// Kartla tahsilat. Tutar, Order saga'sının açtığı ödeme kaydından gelir (istekten değil). Aynı sipariş için aynı anda
    /// tek deneme yapılır: kayıt önce koşullu bir UPDATE ile Processing'e alınır, ikinci istek 409 alır (çift çekim yok).
    /// Sonuç (kayıt durumu, deneme kaydı, PaymentSucceeded/PaymentFailed) tek transaction'da yazılır; event outbox'tan gider.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly PaymentContext _context;
        private readonly GarantiPosClient _posClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(PaymentContext context, GarantiPosClient posClient, IPublishEndpoint publishEndpoint, ILogger<PaymentService> logger)
        {
            _context = context;
            _posClient = posClient;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(string userId, CreatePaymentDto createPaymentDto)
        {
            var payment = await _context.OrderPayments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.OrderNumber == createPaymentDto.OrderNumber);

            // Başkasının siparişi de "bulunamadı" görünür (IDOR). Kayıt, sipariş stok ayrılıp ödeme beklemeye geçince açılır.
            if (payment is null || payment.UserId != userId)
                throw new NotFoundException("Ödeme bekleyen sipariş", createPaymentDto.OrderNumber);

            if (payment.Status != OrderPaymentStatus.AwaitingPayment)
                throw new ConflictException(NotPayableMessage(payment.Status));

            var claimed = await _context.OrderPayments
                .Where(x => x.Id == payment.Id && x.Status == OrderPaymentStatus.AwaitingPayment)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, OrderPaymentStatus.Processing)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
            if (claimed == 0)
                throw new ConflictException("Bu sipariş için başka bir ödeme işlemi sürüyor.");

            // Bankadaki sipariş kimliği her denemede farklı. Sonucu belirsiz kalan deneme sayılmadığı için tekrarında
            // aynı kimlik gider; çekim aslında olduysa banka aynı kimliği ikinci kez kabul etmez.
            var bankOrderId = $"MLT-SHP-{payment.OrderingId}-{payment.FailedAttempts + 1}";
            var card = new CardDetails(createPaymentDto.CardNumber, createPaymentDto.Month, createPaymentDto.Year, createPaymentDto.Cvv);

            GarantiPosResult result;
            try
            {
                result = await _posClient.SaleAsync(bankOrderId, payment.Amount, card);
            }
            catch (Exception ex)
            {
                // Sonuç belirsiz (ya da istek hiç gitmedi, ör. eksik banka ayarı): deneme sayılmaz, kayıt ödeme beklemeye
                // döner (bu arada iptal geldiyse iptal kalır). Aksi halde kayıt Processing'de takılır, sipariş ödenemez.
                await _context.OrderPayments
                    .Where(x => x.Id == payment.Id && x.Status == OrderPaymentStatus.Processing)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Status, OrderPaymentStatus.AwaitingPayment)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
                if (ex is GarantiPosException)
                    throw new ServiceUnavailableException(ex.Message, ex);
                throw;
            }

            try
            {
                return await CompleteAsync(payment, result, createPaymentDto.CardNumber);
            }
            catch (Exception ex)
            {
                // Banka sonuç verdi ama kaydedilemedi: kayıt Processing'de kalır, elle mutabakat gerekir.
                _logger.LogCritical(ex, "Ödeme sonucu kaydedilemedi: sipariş {OrderNumber}, banka sipariş no {BankOrderId}, başarılı: {IsSuccess}.",
                    payment.OrderNumber, bankOrderId, result.IsSuccess);
                throw;
            }
        }

        private async Task<CreatePaymentResponseDto> CompleteAsync(OrderPayment payment, GarantiPosResult result, string cardNumber)
        {
            var now = DateTime.UtcNow;
            var failedAttempts = result.IsSuccess ? payment.FailedAttempts : payment.FailedAttempts + 1;
            var newStatus = result.IsSuccess
                ? OrderPaymentStatus.Succeeded
                : failedAttempts >= PaymentPolicy.MaxAttempts ? OrderPaymentStatus.Failed : OrderPaymentStatus.AwaitingPayment;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Yalnızca hâlâ Processing ise: banka beklenirken sipariş iptal edildiyse kayıt Cancelled kalır.
            var updated = await _context.OrderPayments
                .Where(x => x.Id == payment.Id && x.Status == OrderPaymentStatus.Processing)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.FailedAttempts, failedAttempts)
                    .SetProperty(x => x.PaidAt, result.IsSuccess ? now : (DateTime?)null)
                    .SetProperty(x => x.UpdatedAt, now));

            if (updated == 0)
            {
                newStatus = OrderPaymentStatus.Cancelled;
                if (result.IsSuccess)
                    _logger.LogError("İptal edilmiş sipariş {OrderNumber} için {Amount} TL ödeme alındı; iade gerekli.", payment.OrderNumber, payment.Amount);
            }

            _context.PaymentInfos.Add(new PaymentInfo
            {
                OrderNumber = payment.OrderNumber,
                OrderingId = payment.OrderingId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage is { Length: > 500 } message ? message[..500] : result.ErrorMessage,
                CardNumber = "**** " + cardNumber[^4..],
                CreatedDate = now
            });

            // Para çekildiyse sipariş iptal edilmiş olsa da PaymentSucceeded gider: olan budur, saga iade için loglar.
            if (result.IsSuccess)
                await _publishEndpoint.Publish(new PaymentSucceeded(payment.OrderNumber, payment.Amount, now));
            else
                await _publishEndpoint.Publish(new PaymentFailed(payment.OrderNumber, result.ErrorMessage!));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreatePaymentResponseDto
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                Status = newStatus,
                RemainingAttempts = newStatus == OrderPaymentStatus.AwaitingPayment ? PaymentPolicy.MaxAttempts - failedAttempts : 0
            };
        }

        private static string NotPayableMessage(OrderPaymentStatus status) => status switch
        {
            OrderPaymentStatus.Processing => "Bu sipariş için başka bir ödeme işlemi sürüyor.",
            OrderPaymentStatus.Succeeded => "Bu siparişin ödemesi zaten alındı.",
            OrderPaymentStatus.Failed => "Bu sipariş için ödeme deneme hakkı doldu.",
            OrderPaymentStatus.Cancelled => "Sipariş iptal edildiği için ödeme alınamaz.",
            _ => "Sipariş ödeme beklemiyor."
        };
    }
}
