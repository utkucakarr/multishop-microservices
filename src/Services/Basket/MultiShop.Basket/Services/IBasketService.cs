using MultiShop.Basket.Dtos;

namespace MultiShop.Basket.Services
{
    public interface IBasketService
    {
        /// <summary>Sepeti güncel fiyatlarla döner; satıştan kalkan ürünü ve geçersizleşen kuponu sepetten çıkarır.</summary>
        Task<BasketDto> GetBasketAsync(string userId);

        /// <summary>Sepetteki farklı ürün sayısı (üst menü); yalnızca Redis okur.</summary>
        Task<int> GetItemCountAsync(string userId);

        Task AddItemAsync(string userId, AddBasketItemDto item);

        Task SetQuantityAsync(string userId, string productId, int quantity);

        Task RemoveItemAsync(string userId, string productId);

        Task ApplyCouponAsync(string userId, string code);

        Task RemoveCouponAsync(string userId);

        Task DeleteBasketAsync(string userId);

        /// <summary>
        /// Sepeti onaylar: güncel fiyatlarla yeniden hesaplar, kontrol eder ve <c>BasketCheckedOut</c> yayınlar (sipariş saga'sı başlar).
        /// Sepet silinmez; ödeme başarısız olursa ya da sipariş iptal edilirse kullanıcı sepetini kaybetmez.
        /// </summary>
        Task<CheckoutResultDto> CheckoutAsync(string userId, CheckoutDto request);
    }
}
