namespace MultiShop.DtoLayer.CatalogDtos.ProductImageDtos
{
    // URL ile görsel ekleme (Catalog: POST api/ProductImage). Dosya yükleme ayrı uçtan yapılır.
    public class CreateProductImageDto
    {
        public string ProductId { get; set; }

        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsMain { get; set; }
    }
}
