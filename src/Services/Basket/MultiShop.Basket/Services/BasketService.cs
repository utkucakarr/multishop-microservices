using MultiShop.Basket.Dtos;
using MultiShop.Basket.Settings;
using System.Text.Json;

namespace MultiShop.Basket.Services
{
    public class BasketService : IBasketService
    {
        private readonly RedisService _redisService;

        public BasketService(RedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task DeleteBasket(string userId)
        {
            await _redisService.GetDb().KeyDeleteAsync(userId);
        }

        public async Task<BasketTotalDto> GetBasket(string userId)
        {
            var existBasket = await _redisService.GetDb().StringGetAsync(userId);
            if (existBasket.IsNullOrEmpty)
            {
                // Sepeti yoksa boş bir sepet modeli döndür
                return new BasketTotalDto
                {
                    UserId = userId,
                    DiscountCode = "",
                    DiscountRate = 0,
                    BasketItems = new List<BasketItemDto>() // Sepet elemanları boş
                };
            }
            return JsonSerializer.Deserialize<BasketTotalDto>(existBasket);
        }

        public async Task SaveBasket(BasketTotalDto basketTotalDto)
        {
            await _redisService.GetDb().StringSetAsync(basketTotalDto.UserId, JsonSerializer.Serialize(basketTotalDto));
        }
    }
}
