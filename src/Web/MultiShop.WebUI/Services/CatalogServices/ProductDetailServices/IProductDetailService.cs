using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;

namespace MultiShop.WebUI.Services.CatalogServices.ProductDetailServices
{
    public interface IProductDetailService
    {
        Task<List<ResultProductDetailDto>> GetAllProductDetailAsync();

        /// <summary>Başarılıysa null, değilse servisin hata mesajı.</summary>
        Task<string?> CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto);

        /// <summary>Başarılıysa null, değilse servisin hata mesajı (ör. kısa açıklama boş).</summary>
        Task<string?> UpdateProductDetailAsync(UpdateProductDetailDto updateProductDetailDto);

        Task DeleteProductDetailAsync(string id);

        Task<GetByIdProductDetailDto> GetByIdProductDetailAsync(string id);

        Task<GetByIdProductDetailDto> GetByProductIdProductDetailAsync(string id);
    }
}
