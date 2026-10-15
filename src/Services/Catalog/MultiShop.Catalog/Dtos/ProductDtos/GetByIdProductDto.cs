namespace MultiShop.Catalog.Dtos.ProductDtos
{
    public class GetByIdProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string CategoryId { get; set; }

        public string? BrandId { get; set; }

        public string? BrandName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? ProductDescription { get; set; }
        public int AvailableStock { get; set; }
        /// <summary>Ödemesi beklenen siparişlere ayrılmış; satılabilir = AvailableStock − ReservedStock.</summary>
        public int ReservedStock { get; set; }
        public int RestockThreshold { get; set; }
        public int MaxStockThreshold { get; set; }
        public bool OnReorder { get; set; }
    }
}
