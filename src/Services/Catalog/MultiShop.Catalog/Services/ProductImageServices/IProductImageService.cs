using MultiShop.Catalog.Dtos.ProductImageDtos;

namespace MultiShop.Catalog.Services.ProductImageServices
{
    public interface IProductImageService
    {
        Task<IEnumerable<ResultProductImageDto>> GetAllProductImageAsync();

        Task CreateProductImageAsync(CreateProductImageDto createProductImageDto);

        Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto);

        Task DeleteProductImageAsync(string id);

        Task<GetByIdProductImageDto?> GetByIdProductImageAsync(string id);

        Task<IEnumerable<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id);
    }
}
