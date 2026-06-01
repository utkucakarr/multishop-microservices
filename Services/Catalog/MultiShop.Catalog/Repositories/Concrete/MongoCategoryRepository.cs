using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Repositories.Concrete
{
    public class MongoCategoryRepository : MongoGenericRepository<Category>, ICategoryRepository
    {
        public MongoCategoryRepository(IDatabaseSettings settings) 
            : base(settings, settings.CategoryCollectionName) { }
    }
}
