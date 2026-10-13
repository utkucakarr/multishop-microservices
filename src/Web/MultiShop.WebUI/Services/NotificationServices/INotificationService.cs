using MultiShop.DtoLayer.NotificationDtos;

namespace MultiShop.WebUI.Services.NotificationServices
{
    /// <summary>
    /// Notification servisinin admin uçları (gateway: services/notification/). Okuma metotları servis yanıt
    /// vermezse null/boş döner (üst menü ve sayfalar çökmez); yazma metotları hata mesajı ya da null döner.
    /// </summary>
    public interface INotificationService
    {
        Task<PagedResultDto<NotificationListItemDto>?> GetListAsync(int page, int pageSize, bool unreadOnly);
        Task<NotificationDetailDto?> GetDetailAsync(Guid id);
        Task<int?> GetUnreadCountAsync();
        Task<string?> MarkAsReadAsync(Guid id);
        Task<string?> MarkAllAsReadAsync();
        Task<string?> RetryDeliveryAsync(Guid deliveryId);

        Task<NotificationSettingsDto?> GetSettingsAsync();
        Task<string?> UpdateRulesAsync(List<NotificationRuleDto> rules);
        Task<string?> AddRecipientAsync(CreateNotificationRecipientDto recipient);
        Task<string?> SetRecipientActiveAsync(Guid id, bool isActive);
        Task<string?> DeleteRecipientAsync(Guid id);

        /// <param name="channel">"InApp" | "Email" | "Sms"; null ise "test" türünün kuralı geçerli.</param>
        Task<string?> SendTestAsync(string? channel);
    }
}
