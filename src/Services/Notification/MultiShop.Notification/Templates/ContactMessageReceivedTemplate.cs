using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

/// <summary>İletişim formundan yeni mesaj (KT-10: <c>ContactMessageReceived</c> event'i). Mesaj metni SMS'e konmaz.</summary>
public class ContactMessageReceivedTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.ContactMessageReceived;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var name = data.Get("Name");
        var subject = data.Get("Subject");
        var link = data.TryGetValue("ContactMessageId", out var id) ? $"/Admin/ContactMessage/Detail/{id}" : "/Admin/ContactMessage/Index";
        return new RenderedNotification(
            Title: "Yeni iletişim mesajı",
            Message: $"{name}: {subject}",
            Link: link,
            EmailSubject: $"[MultiShop] Yeni iletişim mesajı: {subject}",
            EmailBody: $"Gönderen: {name} ({data.Get("Email")})\nKonu: {subject}\n\nMesajı admin panelinden okuyabilirsiniz.",
            SmsText: "MultiShop: yeni iletisim mesaji var. Admin panelinden okuyabilirsiniz.");
    }
}
