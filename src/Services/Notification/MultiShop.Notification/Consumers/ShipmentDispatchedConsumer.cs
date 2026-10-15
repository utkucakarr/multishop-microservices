using MassTransit;
using MultiShop.Contracts.Cargo;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Dispatching;

namespace MultiShop.Notification.Consumers;

/// <summary>
/// Gönderi kargoya verildi → müşteriye takip numarasıyla bildirim ("Kargoya verildi" kuralı, varsayılan e-posta).
/// Kuyruk: <c>notification-shipment-dispatched</c>. Alıcı bilgisi event'te (Cargo, sipariş adresinin kopyasından).
/// </summary>
public class ShipmentDispatchedConsumer : IConsumer<ShipmentDispatched>
{
    private readonly INotificationDispatcher _dispatcher;

    public ShipmentDispatchedConsumer(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<ShipmentDispatched> context)
    {
        var message = context.Message;
        if (string.IsNullOrWhiteSpace(message.CustomerEmail))
            return;

        await _dispatcher.DispatchAsync(new NotificationRequest(
            NotificationTypes.OrderShipped,
            new NotificationTarget(NotificationRecipientType.User, message.UserId, message.CustomerEmail),
            new Dictionary<string, string>
            {
                ["OrderNo"] = OrderNumbers.Short(message.OrderNumber),
                ["CargoCompany"] = message.CargoCompanyName,
                ["TrackingNumber"] = message.TrackingNumber,
                ["Name"] = message.CustomerName,
            }), context.CancellationToken);
    }
}
