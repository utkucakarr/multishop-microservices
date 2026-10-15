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

        /// <summary>Sepet fiyatlaması: bulunan ürünlerin güncel fiyatı; olmayan id'ler sonuçta yer almaz.</summary>
        Task<List<ProductPriceDto>> GetPricesByIdsAsync(IReadOnlyCollection<string> ids);

        /// <summary>Admin: eldeki stok. Ayrılmış adetin altına inemez (ödemesi beklenen siparişler etkilenmesin).</summary>
        Task UpdateStockAsync(string id, int availableStock);

        Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryAsync();

        Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryByCategoryIdAsync(string CategoryId);
    }
}
