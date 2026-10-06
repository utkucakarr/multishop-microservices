using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IProductDetailRepository : IGenericRepository<ProductDetail>
    {
        Task<ProductDetail?> GetByProductIdAsync(string productId);
    }
}
