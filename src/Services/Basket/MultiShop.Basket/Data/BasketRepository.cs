using System.Text.Json;
using MultiShop.Basket.Models;
using MultiShop.Basket.Settings;

namespace MultiShop.Basket.Data
{
    /// <summary>Sepetin Redis'teki kaydı. Anahtar <c>basket:{userId}</c>; 30 gün dokunulmayan sepet kendiliğinden silinir (BAS-04).</summary>
    public class BasketRepository
    {
        private static readonly TimeSpan Expiry = TimeSpan.FromDays(30);

        private readonly RedisService _redisService;

        public BasketRepository(RedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<StoredBasket> GetAsync(string userId)
        {
            var value = await _redisService.GetDb().StringGetAsync(Key(userId));
            return value.IsNullOrEmpty
                ? new StoredBasket()
                : JsonSerializer.Deserialize<StoredBasket>(value.ToString()) ?? new StoredBasket();
        }

        /// <summary>Kaydeder ve süreyi yeniler; boş sepet (ürün ve kupon yok) silinir.</summary>
        public async Task SaveAsync(string userId, StoredBasket basket)
        {
            if (basket.Items.Count == 0 && basket.CouponCode is null)
            {
                await DeleteAsync(userId);
                return;
            }

            await _redisService.GetDb().StringSetAsync(Key(userId), JsonSerializer.Serialize(basket), Expiry);
        }

        public Task DeleteAsync(string userId) => _redisService.GetDb().KeyDeleteAsync(Key(userId));

        private static string Key(string userId) => $"basket:{userId}";
    }
}
