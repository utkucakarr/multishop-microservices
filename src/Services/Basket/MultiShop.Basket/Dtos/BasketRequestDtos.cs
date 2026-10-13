using System.ComponentModel.DataAnnotations;

namespace MultiShop.Basket.Dtos
{
    // İstekler yalnızca ürün id'si, adet ve kupon kodu taşır: fiyat, ürün adı ve indirim oranı istemciden alınmaz (BAS-01).

    public class AddBasketItemDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Range(BasketLimits.MinQuantity, BasketLimits.MaxQuantity)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateBasketItemDto
    {
        [Range(BasketLimits.MinQuantity, BasketLimits.MaxQuantity)]
        public int Quantity { get; set; }
    }

    public class ApplyCouponDto
    {
        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty;
    }

    public static class BasketLimits
    {
        public const int MinQuantity = 1;
        public const int MaxQuantity = 99;
    }
}
