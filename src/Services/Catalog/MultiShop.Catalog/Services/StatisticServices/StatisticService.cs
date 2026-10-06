
using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Services.StatisticServices
{
    public class StatisticService : IStatisticService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Brand> _brandCollection;

        public StatisticService(IDatabaseSettings _databaseSettings)
        {
            var client = new MongoClient(_databaseSettings.ConnectionString);
            var database = client.GetDatabase(_databaseSettings.DatabaseName);
            _productCollection = database.GetCollection<Product>(_databaseSettings.ProductCollectionName);
            _categoryCollection = database.GetCollection<Category>(_databaseSettings.CategoryCollectionName);
            _brandCollection = database.GetCollection<Brand>(_databaseSettings.BrandCollectionName);
        }

        public async Task<long> GetBrandCountAsync()
        {
            return await _brandCollection.CountDocumentsAsync(FilterDefinition<Brand>.Empty);
        }

        public async Task<long> GetCategoryCountAsync()
        {
            return await _categoryCollection.CountDocumentsAsync(FilterDefinition<Category>.Empty);
        }

        public async Task<string> GetMaxPriceProductNameAsync()
            => await GetProductNameByPriceAsync(descending: true);

        public async Task<string> GetMinPriceProductNameAsync()
            => await GetProductNameByPriceAsync(descending: false);

        public async Task<decimal> GetProductAvgPriceAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "averagePrice", new BsonDocument("$avg", PriceAsDecimal()) }
                })
            };
            var result = await (await _productCollection.AggregateAsync<BsonDocument>(pipeline)).FirstOrDefaultAsync();
            if (result is null || !result.TryGetValue("averagePrice", out var averagePrice) || averagePrice.IsBsonNull)
                return 0;

            return Math.Round(averagePrice.ToDecimal(), 2);
        }

        private async Task<string> GetProductNameByPriceAsync(bool descending)
        {
            var pipeline = new[]
            {
                new BsonDocument("$addFields", new BsonDocument("priceValue", PriceAsDecimal())),
                new BsonDocument("$match", new BsonDocument("priceValue", new BsonDocument("$ne", BsonNull.Value))),
                new BsonDocument("$sort", new BsonDocument("priceValue", descending ? -1 : 1)),
                new BsonDocument("$limit", 1),
                new BsonDocument("$project", new BsonDocument("ProductName", 1))
            };
            var product = await (await _productCollection.AggregateAsync<BsonDocument>(pipeline)).FirstOrDefaultAsync();
            return product?.GetValue("ProductName", BsonString.Empty).AsString ?? string.Empty;
        }

        // Eski kayıtlarda ProductPrice metin (MongoDB.Driver 2.x), yenilerde Decimal128 (3.x) olarak saklı.
        // Hesaplama ve sıralamanın ikisinde de doğru çalışması için değer sorgu içinde decimal'a çevrilir.
        private static BsonDocument PriceAsDecimal()
            => new BsonDocument("$convert", new BsonDocument
            {
                { "input", "$ProductPrice" },
                { "to", "decimal" },
                { "onError", BsonNull.Value },
                { "onNull", BsonNull.Value }
            });

        public async Task<long> GetProductCountAsync()
        {
            return await _productCollection.CountDocumentsAsync(FilterDefinition<Product>.Empty);
        }
    }
}
