namespace MultiShop.Catalog.Dtos.ProductImageDtos
{
    // multipart/form-data ile gelir: admin panelinden dosya yükleme.
    public class UploadProductImageDto
    {
        public string ProductId { get; set; }
        public IFormFile? File { get; set; }
        public bool IsMain { get; set; }
    }
}
