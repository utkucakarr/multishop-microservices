using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Channels.Email;

public class EmailChannel : INotificationChannel
{
    private readonly IEmailSender _emailSender;

    public EmailChannel(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public NotificationChannel Channel => NotificationChannel.Email;
    public DeliveryMode Mode => DeliveryMode.Outbox;

    public async Task<DeliveryResult> SendAsync(ChannelMessage message, CancellationToken cancellationToken)
    {
        await _emailSender.SendAsync(message.Recipient!, message.Subject ?? message.Notification.Title, message.Body, cancellationToken: cancellationToken);
        return DeliveryResult.Sent;
    }
}
