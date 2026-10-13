using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoCategoryRepository : MongoGenericRepository<Category>, ICategoryRepository
    {
        public MongoCategoryRepository(IMongoDatabase database, IDatabaseSettings settings)
            : base(database, settings.CategoryCollectionName) { }
    }
}
