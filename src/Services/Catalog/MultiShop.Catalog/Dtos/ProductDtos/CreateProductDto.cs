namespace MultiShop.Catalog.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }

        public decimal ProductPrice { get; set; }

        public string? ProductImageUrl { get; set; }

        public string? ProductDescription { get; set; }

        public string CategoryId { get; set; }

        // Ürün sisteme ilk girildiğindeki başlangıç stoğu
        public int AvailableStock { get; set; }

        // Deponun alabileceği maksimum ürün kapasitesi
        public int MaxStockThreshold { get; set; }

        // Yeniden sipariş verme uyarısı için minimum sınır
        public int RestockThreshold { get; set; }
    }
}
