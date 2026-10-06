using MultiShop.Catalog.Dtos.ProductDetailDtos;

namespace MultiShop.Catalog.Services.ProductDetailServices
{
    public interface IProductDetailService
    {
        Task<IEnumerable<ResultProductDetailDto>> GetAllProductDetailAsync();

        Task CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto);

        Task UpdateProductDetailAsyn(UpdateProductDetailDto updateProductDetailDto);

        Task DeleteProductDetailAsync(string id);

        Task<GetByIdProductDetailDto?> GetByIdProductDetailAsync(string id);

        Task<GetByIdProductDetailDto?> GetByProductIdProductDetailAsync(string id);
    }
}
