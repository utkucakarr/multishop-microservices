using MultiShop.Catalog.Dtos.ProductImageDtos;

namespace MultiShop.Catalog.Services.ProductImageServices
{
    public interface IProductImageService
    {
        Task<IEnumerable<ResultProductImageDto>> GetAllProductImageAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateProductImageAsync(CreateProductImageDto createProductImageDto);

        /// <summary>Dosyayı depolamaya kaydeder ve ürüne görsel olarak ekler; ürünün ilk görseli ana görsel olur.</summary>
        Task<GetByIdProductImageDto> UploadProductImageAsync(UploadProductImageDto uploadProductImageDto);

        /// <summary>Görseli ürünün tek ana görseli yapar.</summary>
        Task SetMainProductImageAsync(string id);

        Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto);

        Task DeleteProductImageAsync(string id);

        /// <summary>Ürünün tüm görsellerini (kayıt + dosya) siler; ürün silinirken çağrılır.</summary>
        Task DeleteByProductIdAsync(string productId);

        Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id);

        Task<IEnumerable<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id);
    }
}
