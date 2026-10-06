using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderAddressServices
{
    public class OrderAddressService : IOrderAddressService
    {
        private readonly HttpClient _httpClient;

        public OrderAddressService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CreateOrderAddressAsync(CreateOrderAddressDto createOrderAddressDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync<CreateOrderAddressDto>("addresses", createOrderAddressDto);
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                return jsonData;
            }
            else
            {
                var error = await responseMessage.Content.ReadAsStringAsync();
                // Burada loglama veya özel hata mesajı oluşturabilirsin
                return $"Hata oluştu: {responseMessage.StatusCode} - {error}";
            }
        }
    }
}
