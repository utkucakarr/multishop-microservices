using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

public class TestTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.Test;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var sentBy = data.Get("SentBy");
        return new RenderedNotification(
            Title: "Test bildirimi",
            Message: $"Bildirim kanalları çalışıyor. Gönderen: {sentBy}",
            Link: "/Admin/Notification/Index",
            EmailSubject: "[MultiShop] Test bildirimi",
            EmailBody: $"Bu bir test bildirimidir. Mail kanalı çalışıyor.\n\nGönderen: {sentBy}",
            SmsText: "MultiShop test bildirimi: SMS kanali calisiyor.");
    }
}
