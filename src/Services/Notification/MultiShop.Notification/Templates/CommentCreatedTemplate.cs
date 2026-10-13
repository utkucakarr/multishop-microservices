using MultiShop.Notification.Data;

namespace MultiShop.Notification.Templates;

/// <summary>Yeni ürün yorumu (KT-10: <c>CommentCreated</c> event'i).</summary>
public class CommentCreatedTemplate : INotificationTemplate
{
    public string Type => NotificationTypes.CommentCreated;

    public RenderedNotification Render(IReadOnlyDictionary<string, string> data)
    {
        var name = data.Get("Name");
        return new RenderedNotification(
            Title: "Yeni yorum",
            Message: $"{name} bir ürüne yorum yaptı.",
            Link: "/Admin/Comment/Index",
            EmailSubject: "[MultiShop] Yeni ürün yorumu",
            EmailBody: $"{name} bir ürüne yorum yaptı. Yorumu admin panelinden inceleyebilirsiniz.",
            SmsText: "MultiShop: yeni urun yorumu var.");
    }
}
