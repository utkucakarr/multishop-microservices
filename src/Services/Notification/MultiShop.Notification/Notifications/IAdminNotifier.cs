namespace MultiShop.Notification.Notifications;

/// <summary>
/// Bağlı tüm admin'lere anlık bildirim gönderir. KT-10'daki event consumer'lar (ör. <c>CommentCreated</c>,
/// <c>ContactMessageReceived</c>) bunu çağırır; SignalR ayrıntısını bilmezler.
/// </summary>
public interface IAdminNotifier
{
    Task NotifyAdminsAsync(string type, string title, string message, string? link = null, CancellationToken cancellationToken = default);
}
