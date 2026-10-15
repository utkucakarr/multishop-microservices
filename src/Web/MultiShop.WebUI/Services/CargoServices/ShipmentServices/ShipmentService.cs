using System.Net;
using MultiShop.DtoLayer.CargoDtos.ShipmentDtos;

namespace MultiShop.WebUI.Services.CargoServices.ShipmentServices
{
    public class ShipmentService : IShipmentService
    {
        private readonly HttpClient _httpClient;

        public ShipmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultShipmentDto>> GetAllAsync(string? status)
        {
            var url = string.IsNullOrEmpty(status) ? "shipments" : "shipments?status=" + Uri.EscapeDataString(status);
            return await _httpClient.GetFromJsonAsync<List<ResultShipmentDto>>(url) ?? new();
        }

        public async Task<GetShipmentByIdDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"shipments/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GetShipmentByIdDto>();
        }

        public async Task<string?> DispatchAsync(int id)
            => await (await _httpClient.PostAsync($"shipments/{id}/dispatch", null)).GetErrorMessageAsync();

        public async Task<string?> DeliverAsync(int id)
            => await (await _httpClient.PostAsync($"shipments/{id}/deliver", null)).GetErrorMessageAsync();
    }
}
