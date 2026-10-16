using MassTransit;
using MultiShop.Cargo.BusinessLayer.Services;
using MultiShop.Contracts.Cargo;

namespace MultiShop.Cargo.WebApi.Consumers
{
    /// <summary>
    /// Order saga'sının komutu: ödenen siparişin gönderisini aç. Kuyruk: <c>cargo-create-shipment</c> (<see cref="CargoQueues"/>).
    /// Inbox aynı mesajı ikinci kez işlemez; farklı mesajla gelen aynı sipariş için de gönderi tekrar açılmaz (OrderNumber unique).
    /// </summary>
    public class CreateShipmentConsumer : IConsumer<CreateShipment>
    {
        private readonly ShipmentService _shipmentService;

        public CreateShipmentConsumer(ShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        public Task Consume(ConsumeContext<CreateShipment> context)
            => _shipmentService.CreateAsync(context.Message, context.CancellationToken);
    }
}
