using System.ComponentModel.DataAnnotations;

namespace MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos
{
    public class UpdateProductDetailDto
    {
        // Boşsa ürünün henüz detay kaydı yok demektir; WebUI bu durumda Create çağırır.
        public string? ProductDetailId { get; set; }

        public string ProductId { get; set; }

        [Required(ErrorMessage = "Kısa açıklama zorunludur.")]
        public string ShortDescription { get; set; }

        public string? LongDescription { get; set; }

        public string? ProductInfo { get; set; }
    }
}
