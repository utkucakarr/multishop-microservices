using MultiShop.Catalog.Dtos.ProductImageDtos;

namespace MultiShop.Catalog.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string CategoryId { get; set; }
        public int AvailableStock { get; set; }
        public int MaxStockThreshold { get; set; }
        public int RestockThreshold { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
