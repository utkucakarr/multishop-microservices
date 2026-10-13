using MultiShop.Notification.Notifications;

namespace MultiShop.Notification.Hubs;

/// <summary>
/// İstemcide dinlenen metotlar. Tipli hub sayesinde metot adı string olarak yazılmaz;
/// JavaScript tarafında <c>connection.on("ReceiveNotification", ...)</c> ile karşılanır.
/// </summary>
public interface INotificationClient
{
    Task ReceiveNotification(NotificationMessage notification);
}
