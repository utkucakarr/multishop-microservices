using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

/// <summary>Admin'lere yeni (ödenmiş) sipariş (KT-13: <c>OrderPaid</c> event'i). Varsayılan kanal panel içi.</summary>
public class NewOrderTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.NewOrder;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var orderNo = data.Get("OrderNo");
        var total = data.Get("Total");
        return new RenderedNotification(
            Title: "Yeni sipariş",
            Message: $"{orderNo}: {total} ₺ ({data.Get("Name")})",
            Link: "/Admin/Order/Index",
            EmailSubject: $"[MultiShop] Yeni sipariş ({orderNo})",
            EmailBody: $"{orderNo} numaralı siparişin ödemesi alındı. Tutar: {total} ₺. Gönderisi Kargo → Gönderiler sayfasında.",
            SmsText: $"MultiShop: yeni siparis {orderNo}, {total} TL.");
    }
}
