using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoProductRepository : MongoGenericRepository<Product>, IProductRepository
    {
        // ✅ Category ve Brand collection'ları sadece burada, service'de değil
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Brand> _brandCollection;

        public MongoProductRepository(IMongoDatabase database, IDatabaseSettings settings)
            : base(database, settings.ProductCollectionName)
        {
            _categoryCollection = database.GetCollection<Category>(settings.CategoryCollectionName);
            _brandCollection = database.GetCollection<Brand>(settings.BrandCollectionName);
        }

        public async Task<Product?> GetByIdWithBrandAsync(string id)
        {
            var product = await GetByIdAsync(id);
            if (product is not null)
                await AttachBrandsAsync(new List<Product> { product });
            return product;
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

            await AttachBrandsAsync(products);
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

            await AttachBrandsAsync(products);
            return products;
        }

        public async Task<bool> BrandExistsAsync(string brandId)
            => ObjectId.TryParse(brandId, out _)
               && await _brandCollection.Find(b => b.BrandId == brandId).AnyAsync();

        public async Task<long> CountByBrandIdAsync(string brandId)
            => await _collection.CountDocumentsAsync(p => p.BrandId == brandId);

        // Kategorideki gibi: gereken markalar tek sorguda çekilip bellekte eşleştirilir (N+1 yok).
        private async Task AttachBrandsAsync(List<Product> products)
        {
            var brandIds = products.Where(p => p.BrandId != null).Select(p => p.BrandId!).Distinct().ToList();
            if (brandIds.Count == 0)
                return;

            var brandMap = (await _brandCollection.Find(b => brandIds.Contains(b.BrandId)).ToListAsync())
                .ToDictionary(b => b.BrandId);

            foreach (var product in products)
            {
                if (product.BrandId != null && brandMap.TryGetValue(product.BrandId, out var brand))
                    product.Brand = brand;
            }
        }
    }
}
