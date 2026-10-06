using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoGenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;

        // ✅ Merkezi ObjectId parse metodu
        private static FilterDefinition<T> BuildIdFilter(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                throw new ArgumentException($"Invalid ObjectId format: {id}");

            return Builders<T>.Filter.Eq("_id", objectId);
        }

        public MongoGenericRepository(IDatabaseSettings settings, string collectionName)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task CreateAsync(T entity)
            => await _collection.InsertOneAsync(entity);

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(BuildIdFilter(id));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<T?> GetByIdAsync(string id)
            => await _collection.Find(BuildIdFilter(id)).FirstOrDefaultAsync();

        public async Task UpdateAsync(string id, T entity)
             => await _collection.ReplaceOneAsync(BuildIdFilter(id), entity);
    }
}
