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

        // Verilen ürünlerin ana görsellerini tek sorguda getirir (ProductId -> ImageUrl).
        // Bir üründe birden fazla ana görsel varsa en düşük DisplayOrder'lı olan seçilir.
        public async Task<IDictionary<string, string>> GetMainImageUrlsAsync(IEnumerable<string> productIds)
        {
            var ids = productIds.ToList();
            var mainImages = await _collection
                .Find(x => ids.Contains(x.ProductId) && x.IsMain)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();

            return mainImages
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.First().ImageUrl);
        }

        //public async Task<ProductImage?> GetMainImageByProductIdAsync(string productId)
        //    => await _collection
        //        .Find(x => x.ProductId == productId && x.IsMain)
        //        .FirstOrDefaultAsync();
    }
}
