using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoProductRepository : MongoGenericRepository<Product>, IProductRepository
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        public MongoProductRepository(IDatabaseSettings settings)
            : base(settings, settings.ProductCollectionName)
        {
            // ✅ Category collection sadece burada, service'de değil
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _categoryCollection = database.GetCollection<Category>(
                settings.CategoryCollectionName
            );
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
        {
            var products = await _collection.Find(_ => true).ToListAsync();

            var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();

            // ✅ Tek sorguda tüm gerekli kategorileri çek
            var categories = await _categoryCollection
                .Find(c => categoryIds.Contains(c.CategoryId))
                .ToListAsync();

            // ✅ Memory'de join yap
            var categoryMap = categories.ToDictionary(c => c.CategoryId);

            foreach (var product in products)
            {
                if (categoryMap.TryGetValue(product.CategoryId, out var category))
                    product.Category = category;
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryByCategoryIdAsync(string categoryId)
        {
            var products = await _collection
                    .Find(p => p.CategoryId == categoryId)
                    .ToListAsync();

            // Zaten aynı categoryId — tek sorgu yeterli
            var category = await _categoryCollection
                .Find(c => c.CategoryId == categoryId)
                .FirstOrDefaultAsync();

            foreach (var product in products)
                product.Category = category;

            return products;
        }
    }
}