namespace MultiShop.DtoLayer.CatalogDtos.ProductImageDtos
{
    public class UpdateProductImageDto
    {
        public string ProductImageId { get; set; }

        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsMain { get; set; }
    }
}
