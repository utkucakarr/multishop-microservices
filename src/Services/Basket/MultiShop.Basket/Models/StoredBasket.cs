namespace MultiShop.Basket.Models
{
    /// <summary>
    /// Redis'te saklanan sepet: yalnızca hangi üründen kaç adet ve kupon kodu. Fiyat, ürün adı ve indirim oranı
    /// bilerek saklanmaz; her okumada Catalog ve Discount'tan alınır (istemci fiyatı etkileyemez, fiyat hep güncel).
    /// </summary>
    public class StoredBasket
    {
        public List<StoredBasketItem> Items { get; set; } = new();

        public string? CouponCode { get; set; }
    }

    public class StoredBasketItem
    {
        public string ProductId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
