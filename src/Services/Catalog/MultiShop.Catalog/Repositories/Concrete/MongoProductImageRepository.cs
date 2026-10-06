using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoProductImageRepository : MongoGenericRepository<ProductImage>, IProductImageRepository
    {
        public MongoProductImageRepository(IDatabaseSettings settings) : base(settings, settings.ProductImageCollectionName)
        {
        }

        public async Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(string productId)
                    => await _collection
                        .Find(x => x.ProductId == productId)
                        .SortBy(x => x.DisplayOrder)
                        .ToListAsync();

        //public async Task<ProductImage?> GetMainImageByProductIdAsync(string productId)
        //    => await _collection
        //        .Find(x => x.ProductId == productId && x.IsMain)
        //        .FirstOrDefaultAsync();
    }
}
