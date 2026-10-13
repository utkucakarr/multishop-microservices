using Microsoft.AspNetCore.SignalR;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Hubs;

namespace MultiShop.Notification.Channels.InApp;

/// <summary>
/// Panel içi bildirim. Kalıcı kayıt (NotificationRecord) zaten bildirimin kendisi; buradaki SignalR push yalnızca
/// "yeni bir şey var" ipucu. Kaybolursa istemci bağlanınca/yeniden bağlanınca durumu REST ile yeniden okur.
/// </summary>
public class InAppChannel : INotificationChannel
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public InAppChannel(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public NotificationChannel Channel => NotificationChannel.InApp;
    public DeliveryMode Mode => DeliveryMode.Immediate;

    public async Task<DeliveryResult> SendAsync(ChannelMessage message, CancellationToken cancellationToken)
    {
        var notification = message.Notification;
        var push = new NotificationPush(notification.Id, notification.Type, notification.Title, notification.Message, notification.Link, notification.CreatedAt);

        var clients = notification.RecipientType == NotificationRecipientType.User && notification.RecipientUserId is not null
            ? _hubContext.Clients.User(notification.RecipientUserId)
            : _hubContext.Clients.Group(NotificationHub.AdminsGroup);
        await clients.ReceiveNotification(push);

        return DeliveryResult.Sent;
    }
}
