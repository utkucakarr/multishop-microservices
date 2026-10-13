using Microsoft.AspNetCore.SignalR;
using MultiShop.Notification.Hubs;

namespace MultiShop.Notification.Notifications;

public class SignalRAdminNotifier : IAdminNotifier
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<SignalRAdminNotifier> _logger;

    public SignalRAdminNotifier(IHubContext<NotificationHub, INotificationClient> hubContext, ILogger<SignalRAdminNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyAdminsAsync(string type, string title, string message, string? link = null, CancellationToken cancellationToken = default)
    {
        var notification = new NotificationMessage(type, title, message, link, DateTimeOffset.UtcNow);
        await _hubContext.Clients.Group(NotificationHub.AdminsGroup).ReceiveNotification(notification);
        _logger.LogInformation("Admin bildirimi gönderildi: {NotificationType} {NotificationTitle}", type, title);
    }
}
