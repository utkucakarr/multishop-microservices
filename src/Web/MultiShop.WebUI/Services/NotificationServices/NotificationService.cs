using System.Net;
using MultiShop.DtoLayer.NotificationDtos;

namespace MultiShop.WebUI.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<PagedResultDto<NotificationListItemDto>?> GetListAsync(int page, int pageSize, bool unreadOnly)
            => GetOrDefaultAsync<PagedResultDto<NotificationListItemDto>>($"notifications?page={page}&pageSize={pageSize}&unreadOnly={unreadOnly}");

        public Task<NotificationDetailDto?> GetDetailAsync(Guid id)
            => GetOrDefaultAsync<NotificationDetailDto>($"notifications/{id}");

        public async Task<int?> GetUnreadCountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("notifications/unread-count");
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<int>() : null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Notification servisine ulaşılamadı (okunmamış sayısı).");
                return null;
            }
        }

        public Task<string?> MarkAsReadAsync(Guid id)
            => SendAsync(() => _httpClient.PutAsync($"notifications/{id}/read", null));

        public Task<string?> MarkAllAsReadAsync()
            => SendAsync(() => _httpClient.PutAsync("notifications/read-all", null));

        public Task<string?> RetryDeliveryAsync(Guid deliveryId)
            => SendAsync(() => _httpClient.PostAsync($"deliveries/{deliveryId}/retry", null));

        public Task<NotificationSettingsDto?> GetSettingsAsync()
            => GetOrDefaultAsync<NotificationSettingsDto>("notification-settings");

        public Task<string?> UpdateRulesAsync(List<NotificationRuleDto> rules)
            => SendAsync(() => _httpClient.PutAsJsonAsync("notification-settings/rules", rules));

        public Task<string?> AddRecipientAsync(CreateNotificationRecipientDto recipient)
            => SendAsync(() => _httpClient.PostAsJsonAsync("notification-settings/recipients", recipient));

        public Task<string?> SetRecipientActiveAsync(Guid id, bool isActive)
            => SendAsync(() => _httpClient.PutAsJsonAsync($"notification-settings/recipients/{id}", new { isActive }));

        public Task<string?> DeleteRecipientAsync(Guid id)
            => SendAsync(() => _httpClient.DeleteAsync($"notification-settings/recipients/{id}"));

        public Task<string?> SendTestAsync(string? channel)
            => SendAsync(() => _httpClient.PostAsync(string.IsNullOrEmpty(channel) ? "notifications/test" : $"notifications/test?channel={Uri.EscapeDataString(channel)}", null));

        private async Task<T?> GetOrDefaultAsync<T>(string url) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
                    return null;
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Notification servisine ulaşılamadı: {Url}", url);
                return null;
            }
        }

        private async Task<string?> SendAsync(Func<Task<HttpResponseMessage>> send)
        {
            try
            {
                var response = await send();
                return await response.GetErrorMessageAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Notification servisine ulaşılamadı.");
                return "Bildirim servisine ulaşılamadı.";
            }
        }
    }
}
