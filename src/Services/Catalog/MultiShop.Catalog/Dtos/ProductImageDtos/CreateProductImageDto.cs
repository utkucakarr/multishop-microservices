namespace MultiShop.Catalog.Dtos.ProductImageDtos
{
    public class CreateProductImageDto
    {
        public string ProductId { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
    }
}
