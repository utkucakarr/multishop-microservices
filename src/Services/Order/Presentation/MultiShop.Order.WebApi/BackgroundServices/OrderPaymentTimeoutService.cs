using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Persistence.Context;

namespace MultiShop.Order.WebApi.BackgroundServices
{
    /// <summary><c>OrderSettings</c> ayar bölümü.</summary>
    public class OrderSettings
    {
        public const string SectionName = "OrderSettings";

        /// <summary>Sipariş bu kadar dakika içinde ödenmezse iptal edilir ve ayrılan stok bırakılır.</summary>
        public int PaymentTimeoutMinutes { get; set; } = 30;
    }

    /// <summary>
    /// Ödenmeyen siparişlerin zaman aşımı: dakikada bir, süresi geçmiş Pending/AwaitingPayment siparişlere saga üzerinden
    /// <see cref="CancelOrder"/> gönderir. Sepet onaydan sonra silinmediği için kullanıcı ödeme sayfasını kapatıp yeniden
    /// onaylayabilir; zaman aşımı olmasa ilk sipariş stoğu süresiz kilitli tutardı.
    /// Komut outbox'a yazılır; saga iptal edince Payment da bekleyen ödemeyi kapatır.
    /// </summary>
    public class OrderPaymentTimeoutService : BackgroundService
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<OrderSettings> _settings;
        private readonly ILogger<OrderPaymentTimeoutService> _logger;

        public OrderPaymentTimeoutService(IServiceScopeFactory scopeFactory, IOptionsMonitor<OrderSettings> settings, ILogger<OrderPaymentTimeoutService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    await CancelExpiredOrdersAsync(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // Veritabanı geçici olarak yoksa servis durmaz; bir sonraki turda tekrar dener.
                    _logger.LogWarning(ex, "Ödenmeyen siparişler kontrol edilemedi.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task CancelExpiredOrdersAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromMinutes(_settings.CurrentValue.PaymentTimeoutMinutes);
            var cutoff = DateTime.UtcNow - timeout;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            // Saga durum adları: OrderStateMachine'deki State özellikleri.
            var expired = await context.Set<OrderState>()
                .Where(x => (x.CurrentState == "Pending" || x.CurrentState == "AwaitingPayment") && x.CreatedAt < cutoff)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.CorrelationId)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (expired.Count == 0)
                return;

            var reason = $"Ödeme {timeout.TotalMinutes:0} dakika içinde yapılmadığı için sipariş iptal edildi.";
            foreach (var orderNumber in expired)
                await publishEndpoint.Publish(new CancelOrder(orderNumber, reason), cancellationToken);

            // Bus outbox: komutlar OutboxMessage'a bu SaveChanges ile yazılır. Saga bir sonraki turdan önce işlemezse
            // komut tekrar gider; saga Cancelled durumunda CancelOrder'ı yok sayar.
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Count} ödenmeyen sipariş zaman aşımıyla iptale gönderildi.", expired.Count);
        }
    }
}
