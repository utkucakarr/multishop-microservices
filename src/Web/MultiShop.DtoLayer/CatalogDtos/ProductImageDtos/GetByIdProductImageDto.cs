namespace MultiShop.DtoLayer.CatalogDtos.ProductImageDtos
{
    // Catalog'daki ProductImage yapısıyla aynı: her görsel ayrı bir kayıt.
    public class GetByIdProductImageDto
    {
        public string ProductImageId { get; set; }

        public string ProductId { get; set; }

        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsMain { get; set; }
    }
}
