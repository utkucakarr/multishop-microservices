using MultiShop.DtoLayer.StatisticDtos;

namespace MultiShop.WebUI.Services.StatisticServices
{
    // BaseAddress: {OcelotUrl}/services/ — dashboard aggregate'i ve servislerin tekil istatistik route'ları.
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(HttpClient httpClient, ILogger<DashboardService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
            => await GetAsync<AdminDashboardDto>("admin/dashboard") ?? new AdminDashboardDto();

        public async Task<CommentStatisticsDto?> GetCommentStatisticsAsync()
            => await GetAsync<CommentStatisticsDto>("comment/statistics");

        public async Task<MessageStatisticsDto?> GetMessageStatisticsAsync()
            => await GetAsync<MessageStatisticsDto>("message/statistics");

        private async Task<T?> GetAsync<T>(string path) where T : class
        {
            var responseMessage = await _httpClient.GetAsync(path);
            if (responseMessage.IsSuccessStatusCode)
                return await responseMessage.Content.ReadFromJsonAsync<T>();

            _logger.LogWarning("İstatistik alınamadı: {Path} → {StatusCode}", path, (int)responseMessage.StatusCode);
            return null;
        }
    }
}
