using MultiShop.DtoLayer.BasketDtos;

namespace MultiShop.WebUI.Services.BasketServices
{
    /// <summary>
    /// Basket servisinin istemcisi. Yalnızca ürün id'si, adet ve kupon kodu gönderilir; fiyat ve toplamlar Basket'ten okunur.
    /// Değiştiren metotlar başarıda null, hatada Basket'in mesajını (ör. "Kupon geçersiz veya süresi dolmuş.") döner.
    /// </summary>
    public interface IBasketService
    {
        /// <summary>Fiyatlanmış sepet; Basket'e ulaşılamazsa null.</summary>
        Task<BasketDto?> GetBasketAsync();

        /// <summary>Üst menüdeki rozet; hata olursa 0.</summary>
        Task<int> GetItemCountAsync();

        Task<string?> AddItemAsync(string productId, int quantity = 1);

        Task<string?> SetQuantityAsync(string productId, int quantity);

        Task<string?> RemoveItemAsync(string productId);

        Task<string?> ApplyCouponAsync(string code);

        Task<string?> RemoveCouponAsync();

        Task DeleteBasketAsync();

        /// <summary>
        /// Sepeti onaylar; Basket siparişi başlatır (sepet silinmez). Başarıda sipariş numarası.
        /// <c>BasketChanged</c>: sepet ya da tutar onaydan önce değişti (409), kullanıcı sepeti yeniden görmeli.
        /// </summary>
        Task<(Guid? OrderNumber, string? Error, bool BasketChanged)> CheckoutAsync(int addressId, decimal expectedTotal);
    }
}
