namespace MultiShop.Notification.Dispatching;

public interface INotificationDispatcher
{
    /// <summary>
    /// Bildirimi kaydeder ve kanallara dağıtır. Dönen değer bildirimin id'si; kuralda hiçbir kanal açık değilse null.
    /// </summary>
    Task<Guid?> DispatchAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
