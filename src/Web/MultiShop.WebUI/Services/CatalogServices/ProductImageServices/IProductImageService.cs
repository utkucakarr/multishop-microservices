using MultiShop.DtoLayer.CatalogDtos.ProductImageDtos;

namespace MultiShop.WebUI.Services.CatalogServices.ProductImageServices
{
    public interface IProductImageService
    {
        Task<List<ResultProductImageDto>> GetAllProductImageAsync();

        Task CreateProductImageAsync(CreateProductImageDto createProductImageDto);

        Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto);

        /// <summary>Dosyayı Catalog'a yükler; hata varsa kullanıcıya gösterilecek mesajı, yoksa null döner.</summary>
        Task<string?> UploadProductImageAsync(string productId, IFormFile file, bool isMain);

        Task<string?> SetMainProductImageAsync(string id);

        Task<string?> DeleteProductImageAsync(string id);

        Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id);

        Task<List<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id);
    }
}
