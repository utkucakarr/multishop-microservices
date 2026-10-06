using Humanizer;
using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.OrderServices.OrderOrderingServices
{
    public class OrderOrderingService : IOrderOrderingService
    {
        private readonly HttpClient _httpClient;

        public OrderOrderingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int> CreateOrderingAsync(CreateOrderingDto createOrderingDto)
        {
            var response = await _httpClient.PostAsJsonAsync("ordering", createOrderingDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<ResultOrderingByUserIdDto>> GetOrderingByUserId(string id)
        {
            var responseMessage = await _httpClient.GetAsync("ordering/GetOrderingByUserId?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultOrderingByUserIdDto>>(jsonData);
            return values;
        }
    }
}
