namespace MultiShop.Catalog.Dtos.ProductDtos
{
    /// <summary>Sepetin fiyatlanması için ürünün güncel adı, fiyatı ve görseli (Basket servisi kullanır).</summary>
    public class ProductPriceDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
