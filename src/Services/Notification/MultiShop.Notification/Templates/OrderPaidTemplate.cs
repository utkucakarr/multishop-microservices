using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

/// <summary>Müşteriye sipariş onayı (KT-13: <c>OrderPaid</c> event'i). Varsayılan kanal e-posta.</summary>
public class OrderPaidTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.OrderPaid;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var orderNo = data.Get("OrderNo");
        var total = data.Get("Total");
        return new RenderedNotification(
            Title: "Siparişiniz alındı",
            Message: $"{orderNo} numaralı siparişinizin ödemesi alındı ({total} ₺).",
            Link: "/User/MyOrder/MyOrderList",
            EmailSubject: $"[MultiShop] Siparişiniz alındı ({orderNo})",
            EmailBody: $"Merhaba {data.Get("Name")},\n\n{orderNo} numaralı siparişinizin ödemesi alındı. Tutar: {total} ₺.\n"
                + "Siparişiniz hazırlanıyor; kargoya verildiğinde takip numarasıyla size tekrar haber vereceğiz.\n\n"
                + "Siparişlerinizi sitemizdeki \"Siparişlerim\" sayfasından takip edebilirsiniz.\n\nMultiShop",
            SmsText: $"MultiShop: {orderNo} numarali siparisinizin odemesi alindi.");
    }
}
