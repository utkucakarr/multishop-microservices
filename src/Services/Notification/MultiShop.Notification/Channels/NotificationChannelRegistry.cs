using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Channels;

public class NotificationChannelRegistry
{
    private readonly Dictionary<NotificationChannel, INotificationChannel> _channels;

    public NotificationChannelRegistry(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels.ToDictionary(x => x.Channel);
    }

    public INotificationChannel Get(NotificationChannel channel)
        => _channels.TryGetValue(channel, out var strategy)
            ? strategy
            : throw new InvalidOperationException($"'{channel}' kanalı için strateji kayıtlı değil.");
}
