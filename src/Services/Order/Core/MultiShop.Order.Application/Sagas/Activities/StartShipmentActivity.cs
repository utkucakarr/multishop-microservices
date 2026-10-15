using MassTransit;
using Microsoft.Extensions.Logging;
using MultiShop.Contracts.Cargo;
using MultiShop.Contracts.Orders;
using MultiShop.Contracts.Payments;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Sagas.Activities
{
    /// <summary>
    /// Ödeme alındı: siparişin teslimat adresiyle Cargo'ya <see cref="CreateShipment"/> komutu gönderilir (orkestrasyon:
    /// gönderi açmak saga'nın bir sonraki adımı) ve <see cref="OrderPaid"/> yayınlanır (yan işler: Notification onay maili).
    /// Adres mesajlara kopyalanır (event-carried state transfer): Cargo ve Notification Order'a sormaz. İkisi de outbox'tan,
    /// durum değişikliğiyle aynı transaction'da gider.
    /// </summary>
    public class StartShipmentActivity : IStateMachineActivity<OrderState, PaymentSucceeded>
    {
        private static readonly Uri CreateShipmentQueue = new($"queue:{CargoQueues.CreateShipment}");

        private readonly IRepository<Ordering> _orderingRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly ILogger<StartShipmentActivity> _logger;

        public StartShipmentActivity(IRepository<Ordering> orderingRepository, IRepository<Address> addressRepository, ILogger<StartShipmentActivity> logger)
        {
            _orderingRepository = orderingRepository;
            _addressRepository = addressRepository;
            _logger = logger;
        }

        public async Task Execute(BehaviorContext<OrderState, PaymentSucceeded> context, IBehavior<OrderState, PaymentSucceeded> next)
        {
            var saga = context.Saga;
            var ordering = await _orderingRepository.GetByIdAsync(saga.OrderingId)
                ?? throw new InvalidOperationException($"Saga {saga.CorrelationId} için sipariş bulunamadı (OrderingId {saga.OrderingId}).");

            var address = await _addressRepository.GetByIdAsync(ordering.AddressId);
            if (address is null)
            {
                // Kullanıcı adresi ödemeden önce silmiş olabilir: gönderi yine açılır, admin panelinde eksik adres görünür.
                _logger.LogWarning("Sipariş {OrderNumber} için teslimat adresi {AddressId} bulunamadı; gönderi adressiz açılıyor.",
                    saga.CorrelationId, ordering.AddressId);
            }

            var recipient = new ShipmentRecipient(
                Name: address?.Name ?? "-",
                Surname: address?.SurName ?? "-",
                Email: address?.Email ?? string.Empty,
                Phone: address?.Phone ?? "-",
                AddressLine1: address?.Detail1 ?? "-",
                AddressLine2: string.IsNullOrWhiteSpace(address?.Detail2) ? null : address.Detail2,
                District: address?.District ?? "-",
                City: address?.City ?? "-",
                Country: address?.Country ?? "-",
                ZipCode: string.IsNullOrWhiteSpace(address?.ZipCode) ? null : address.ZipCode);

            var sendEndpoint = await context.GetSendEndpoint(CreateShipmentQueue);
            await sendEndpoint.Send(new CreateShipment(saga.CorrelationId, saga.OrderingId, saga.UserId, recipient));

            await context.Publish(new OrderPaid(
                saga.CorrelationId,
                saga.OrderingId,
                saga.UserId,
                saga.TotalPrice,
                context.Message.PaidAt,
                $"{recipient.Name} {recipient.Surname}".Trim(),
                recipient.Email));

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, PaymentSucceeded, TException> context, IBehavior<OrderState, PaymentSucceeded> next)
            where TException : Exception
            => next.Faulted(context);

        public void Probe(ProbeContext context) => context.CreateScope("start-shipment");

        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    }
}
