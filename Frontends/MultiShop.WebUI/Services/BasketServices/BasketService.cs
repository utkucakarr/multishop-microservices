using System.Text.Json;
using MultiShop.DtoLayer.BasketDtos;

namespace MultiShop.WebUI.Services.BasketServices
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;

        public BasketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddBasketBtnItem(BasketItemDto basketItemDto)
        {
            var currentBasket = await GetBasket();
            if (currentBasket is null)
            {
                currentBasket = new BasketTotalDto();
                currentBasket.BasketItems.Add(basketItemDto);
            }
            else
            {
                var searchedBasketItem = currentBasket.BasketItems.FirstOrDefault(x => x.ProductId == basketItemDto.ProductId);
                if (searchedBasketItem is not null)
                {
                    searchedBasketItem.Quantity += 1;
                }
                else
                {
                    currentBasket.BasketItems.Add(basketItemDto);
                }
            }

            await SaveBasket(currentBasket);
        }

        public async Task AddBasketItem(BasketItemDto basketItemDto)
        {
            var values = await GetBasket();
            if (values != null)
            {
                if (!values.BasketItems.Any(x => x.ProductId == basketItemDto.ProductId)) // uyan değer var mı diye bakıyor.
                {
                    values.BasketItems.Add(basketItemDto);
                }
                else
                {
                    values = new BasketTotalDto();
                    values.BasketItems.Add(basketItemDto);
                }
            }
            await SaveBasket(values);
        }

        public async Task DeleteBasket(string userId)
        {
            var responseMessage = await _httpClient.DeleteAsync("baskets");
        }

        public async Task<BasketTotalDto> GetBasket()
        {
            var responseMessage = await _httpClient.GetAsync("baskets");
            var content = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }
            return JsonSerializer.Deserialize<BasketTotalDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> RemoveAllBasketItem(string productId)
        {
            var values = await GetBasket();
            var deletedItem = values.BasketItems.FirstOrDefault(x=>x.ProductId == productId);
            var result = values.BasketItems.Remove(deletedItem);
            await SaveBasket(values);
            return true;
        }

        public async Task RemoveBasketItem (string productId)
        {
            var currentBasket = await GetBasket();
            var deletedItem = currentBasket.BasketItems.FirstOrDefault(x => x.ProductId == productId);
            if (deletedItem != null && deletedItem.Quantity > 1)
            {
                deletedItem.Quantity--;
                await SaveBasket(currentBasket);
            }
        }

        public async Task SaveBasket(BasketTotalDto basketTotalDto)
        {
            await _httpClient.PostAsJsonAsync<BasketTotalDto>("baskets", basketTotalDto);
        }
    }
}
