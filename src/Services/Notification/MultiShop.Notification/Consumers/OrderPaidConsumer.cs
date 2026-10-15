using System.Globalization;
using MassTransit;
using MultiShop.Contracts.Orders;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Dispatching;

namespace MultiShop.Notification.Consumers;

/// <summary>
/// Sipariş ödendi → müşteriye onay ("Sipariş onayı" kuralı, varsayılan e-posta) ve admin'lere "Yeni sipariş" (varsayılan panel içi).
/// İki ayrı tür: admin kanalları müşteri kanallarından bağımsız açılıp kapatılır. Kuyruk: <c>notification-order-paid</c>.
/// Müşterinin adı ve e-postası event'te (event-carried state transfer): Notification Identity'ye ya da Order'a sormaz.
/// </summary>
public class OrderPaidConsumer : IConsumer<OrderPaid>
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    private readonly INotificationDispatcher _dispatcher;

    public OrderPaidConsumer(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<OrderPaid> context)
    {
        var message = context.Message;
        var data = new Dictionary<string, string>
        {
            ["OrderNo"] = OrderNumbers.Short(message.OrderNumber),
            ["Total"] = message.TotalPrice.ToString("N2", Turkish),
            ["Name"] = message.CustomerName,
        };

        // Teslimat adresi ödemeden önce silindiyse e-posta boş gelir: müşteri bildirimi atlanır, admin'inki gider.
        if (!string.IsNullOrWhiteSpace(message.CustomerEmail))
        {
            await _dispatcher.DispatchAsync(new NotificationRequest(
                NotificationTypes.OrderPaid,
                new NotificationTarget(NotificationRecipientType.User, message.UserId, message.CustomerEmail),
                data), context.CancellationToken);
        }

        await _dispatcher.DispatchAsync(new NotificationRequest(NotificationTypes.NewOrder, NotificationTarget.Admins, data), context.CancellationToken);
    }
}

/// <summary>Sipariş numarasının ekranlarda gösterilen kısa hali (WebUI ile aynı: ilk 8 karakter, büyük harf).</summary>
internal static class OrderNumbers
{
    public static string Short(Guid orderNumber) => orderNumber.ToString()[..8].ToUpperInvariant();
}
