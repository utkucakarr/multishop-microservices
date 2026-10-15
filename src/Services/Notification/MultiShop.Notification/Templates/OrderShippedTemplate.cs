using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

/// <summary>Müşteriye kargo bildirimi (KT-13: <c>ShipmentDispatched</c> event'i). Varsayılan kanal e-posta.</summary>
public class OrderShippedTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.OrderShipped;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var orderNo = data.Get("OrderNo");
        var company = data.Get("CargoCompany");
        var tracking = data.Get("TrackingNumber");
        return new RenderedNotification(
            Title: "Siparişiniz kargoya verildi",
            Message: $"{orderNo}: {company}, takip no {tracking}",
            Link: "/User/MyOrder/MyOrderList",
            EmailSubject: $"[MultiShop] Siparişiniz kargoya verildi ({orderNo})",
            EmailBody: $"Merhaba {data.Get("Name")},\n\n{orderNo} numaralı siparişiniz {company} ile kargoya verildi.\n"
                + $"Takip numarası: {tracking}\n\nMultiShop",
            SmsText: $"MultiShop: {orderNo} numarali siparisiniz kargoda. Takip no: {tracking}");
    }
}
