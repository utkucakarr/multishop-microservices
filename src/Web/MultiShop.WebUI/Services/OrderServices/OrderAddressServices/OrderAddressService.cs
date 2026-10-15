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

        public async Task<(int? AddressId, string? Error)> CreateOrderAddressAsync(CreateOrderAddressDto createOrderAddressDto)
        {
            var response = await _httpClient.PostAsJsonAsync("addresses", createOrderAddressDto);
            var error = await response.GetErrorMessageAsync();
            if (error is not null)
                return (null, error);

            var created = await response.Content.ReadFromJsonAsync<CreatedAddress>();
            return (created?.AddressId, created is null ? "Adres kaydedilemedi." : null);
        }

        public async Task<List<ResultOrderAddressDto>> GetMyAddressesAsync()
            => await _httpClient.GetFromJsonAsync<List<ResultOrderAddressDto>>("addresses/mine") ?? new();

        // Order'ın POST addresses yanıtı: { "addressId": 5 }
        private record CreatedAddress(int AddressId);
    }
}
