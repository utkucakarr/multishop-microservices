using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;

namespace MultiShop.WebUI.Services.CatalogServices.ProductDetailServices
{
    public interface IProductDetailService
    {
        /// <summary>Başarılıysa null, değilse servisin hata mesajı.</summary>
        Task<string?> CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto);

        /// <summary>Başarılıysa null, değilse servisin hata mesajı (ör. kısa açıklama boş).</summary>
        Task<string?> UpdateProductDetailAsync(UpdateProductDetailDto updateProductDetailDto);

        /// <summary>Ürünün detay kaydı; yoksa boş nesne (alanları null).</summary>
        Task<GetByIdProductDetailDto> GetByProductIdProductDetailAsync(string id);
    }
}
