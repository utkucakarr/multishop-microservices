using MultiShop.Notification.Dtos;

namespace MultiShop.Notification.Services;

/// <summary>Bildirim Ayarları: tür × kanal kuralları ve admin bildirimlerinin alıcıları.</summary>
public interface INotificationSettingsService
{
    Task<NotificationSettingsDto> GetAsync(CancellationToken cancellationToken);
    Task UpdateRulesAsync(IReadOnlyCollection<UpdateNotificationRuleDto> rules, CancellationToken cancellationToken);
    Task<NotificationRecipientDto> AddRecipientAsync(CreateNotificationRecipientDto recipient, CancellationToken cancellationToken);
    Task SetRecipientActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task DeleteRecipientAsync(Guid id, CancellationToken cancellationToken);
}
