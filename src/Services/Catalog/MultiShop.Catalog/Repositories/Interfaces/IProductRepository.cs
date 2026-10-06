using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<IEnumerable<Product>> GetProductsWithCategoryByCategoryIdAsync(string categoryId);
    }
}
