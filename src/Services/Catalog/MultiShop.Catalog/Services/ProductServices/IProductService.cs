using MultiShop.Catalog.Dtos.ProductDtos;

namespace MultiShop.Catalog.Services.ProductServices
{
    public interface IProductService
    {
        Task<IEnumerable<ResultProductDto>> GetAllProductAsync();

        Task CreateProductAsync(CreateProductDto createProductDto);

        Task UpdateProductAsync(UpdateProductDto updateProductDto);

        Task DeleteProductAsync(string id);

        Task<GetByIdProductDto> GetByIdProductAsync(string id);

        Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryAsync();

        Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryByCategoryIdAsync(string CategoryId);
    }
}
