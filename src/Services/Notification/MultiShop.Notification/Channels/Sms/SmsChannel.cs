using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Channels.Sms;

public class SmsChannel : INotificationChannel
{
    private readonly ISmsSender _smsSender;

    public SmsChannel(ISmsSender smsSender)
    {
        _smsSender = smsSender;
    }

    public NotificationChannel Channel => NotificationChannel.Sms;
    public DeliveryMode Mode => DeliveryMode.Outbox;

    public async Task<DeliveryResult> SendAsync(ChannelMessage message, CancellationToken cancellationToken)
    {
        var result = await _smsSender.SendAsync(message.Recipient!, message.Body, cancellationToken);
        return result == SmsSendResult.Simulated ? DeliveryResult.Simulated : DeliveryResult.Sent;
    }
}
