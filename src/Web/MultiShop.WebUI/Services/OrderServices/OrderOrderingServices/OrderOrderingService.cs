using System.Net;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderOrderingServices
{
    public class OrderOrderingService : IOrderOrderingService
    {
        private readonly HttpClient _httpClient;

        public OrderOrderingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultOrderingByUserIdDto>> GetMyOrderingsAsync()
            => await _httpClient.GetFromJsonAsync<List<ResultOrderingByUserIdDto>>("ordering/mine") ?? new();

        public async Task<ResultOrderingByUserIdDto?> GetByNumberAsync(Guid orderNumber)
        {
            var response = await _httpClient.GetAsync($"ordering/by-number/{orderNumber}");
            // Saga sepet onayını henüz işlemediyse sipariş yoktur; başkasının siparişi de 404 döner.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResultOrderingByUserIdDto>();
        }

        public async Task<string?> CancelAsync(Guid orderNumber)
            => await (await _httpClient.PostAsync($"ordering/{orderNumber}/cancel", null)).GetErrorMessageAsync();

        public async Task<List<ResultOrderingByUserIdDto>> GetAllAsync(string? status)
        {
            var url = string.IsNullOrEmpty(status) ? "ordering" : "ordering?status=" + Uri.EscapeDataString(status);
            return await _httpClient.GetFromJsonAsync<List<ResultOrderingByUserIdDto>>(url) ?? new();
        }
    }
}
