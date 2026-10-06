using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoProductDetailRepository : MongoGenericRepository<ProductDetail>, IProductDetailRepository
    {
        public MongoProductDetailRepository(IDatabaseSettings settings)
            : base(settings, settings.ProductDetailCollectionName) { }

        public async Task<ProductDetail?> GetByProductIdAsync(string productId)
            => await _collection
                .Find(x => x.ProductId == productId)
                .FirstOrDefaultAsync();
    }
}
