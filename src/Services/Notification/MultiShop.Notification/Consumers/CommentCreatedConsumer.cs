using MassTransit;
using MultiShop.Contracts.Comments;
using MultiShop.Notification.Data;
using MultiShop.Notification.Dispatching;

namespace MultiShop.Notification.Consumers;

/// <summary>
/// Yeni ürün yorumu → admin'lere bildirim (kanallar "Yeni yorum" kuralından). Kuyruk: <c>notification-comment-created</c>.
/// Inbox sayesinde aynı event iki kez gelse de bildirim bir kez oluşur.
/// </summary>
public class CommentCreatedConsumer : IConsumer<CommentCreated>
{
    private readonly INotificationDispatcher _dispatcher;

    public CommentCreatedConsumer(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<CommentCreated> context)
    {
        var message = context.Message;
        var request = new NotificationRequest(
            NotificationTypes.CommentCreated,
            NotificationTarget.Admins,
            new Dictionary<string, string> { ["Name"] = message.NameSurname });

        await _dispatcher.DispatchAsync(request, context.CancellationToken);
    }
}
