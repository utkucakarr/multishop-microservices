using MultiShop.Notification.Dtos;

namespace MultiShop.Notification.Services;

/// <summary>Admin panelinin bildirim kutusu: liste, detay, okundu bilgisi (ortak), başarısız gönderimi tekrar deneme.</summary>
public interface INotificationInboxService
{
    Task<PagedResultDto<NotificationListItemDto>> GetListAsync(int page, int pageSize, string? type, bool unreadOnly, CancellationToken cancellationToken);
    Task<NotificationDetailDto> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid id, string readBy, CancellationToken cancellationToken);
    Task<int> MarkAllAsReadAsync(string readBy, CancellationToken cancellationToken);
    Task RetryDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken);
}
