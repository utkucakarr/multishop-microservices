using MultiShop.DtoLayer.BasketDtos;

namespace MultiShop.WebUI.Services.BasketServices
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BasketService> _logger;

        public BasketService(HttpClient httpClient, ILogger<BasketService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<BasketDto?> GetBasketAsync()
        {
            var response = await _httpClient.GetAsync("baskets");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Sepet alınamadı ({StatusCode}): {Error}", response.StatusCode, await response.GetErrorMessageAsync());
                return null;
            }
            return await response.Content.ReadFromJsonAsync<BasketDto>();
        }

        public async Task<int> GetItemCountAsync()
        {
            var response = await _httpClient.GetAsync("baskets/count");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<int>() : 0;
        }

        public async Task<string?> AddItemAsync(string productId, int quantity = 1)
            => await (await _httpClient.PostAsJsonAsync("baskets/items", new { productId, quantity })).GetErrorMessageAsync();

        public async Task<string?> SetQuantityAsync(string productId, int quantity)
            => await (await _httpClient.PutAsJsonAsync("baskets/items/" + Uri.EscapeDataString(productId), new { quantity })).GetErrorMessageAsync();

        public async Task<string?> RemoveItemAsync(string productId)
            => await (await _httpClient.DeleteAsync("baskets/items/" + Uri.EscapeDataString(productId))).GetErrorMessageAsync();

        public async Task<string?> ApplyCouponAsync(string code)
            => await (await _httpClient.PutAsJsonAsync("baskets/coupon", new { code })).GetErrorMessageAsync();

        public async Task<string?> RemoveCouponAsync()
            => await (await _httpClient.DeleteAsync("baskets/coupon")).GetErrorMessageAsync();

        public async Task<(Guid? OrderNumber, string? Error, bool BasketChanged)> CheckoutAsync(int addressId, decimal expectedTotal)
        {
            var response = await _httpClient.PostAsJsonAsync("baskets/checkout", new CheckoutDto { AddressId = addressId, ExpectedTotal = expectedTotal });
            var error = await response.GetErrorMessageAsync();
            if (error is not null)
                return (null, error, response.StatusCode == System.Net.HttpStatusCode.Conflict);

            var result = await response.Content.ReadFromJsonAsync<CheckoutResultDto>();
            return (result?.OrderNumber, result is null ? "Sipariş oluşturulamadı." : null, false);
        }

        public async Task DeleteBasketAsync()
        {
            var response = await _httpClient.DeleteAsync("baskets");
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Sepet silinemedi ({StatusCode})", response.StatusCode);
        }
    }
}
