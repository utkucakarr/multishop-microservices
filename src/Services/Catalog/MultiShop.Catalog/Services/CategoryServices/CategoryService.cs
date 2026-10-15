using AutoMapper;
using MongoDB.Driver;
using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Exceptions;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Catalog.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public CategoryService(IMapper mapper, IMongoDatabase database, IDatabaseSettings _databaseSettings, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _categoryCollection = database.GetCollection<Category>(_databaseSettings.CategoryCollectionName);
            _mapper = mapper;
        }

        public async Task<string> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var value = _mapper.Map<Category>(createCategoryDto); //UIdan gelen veriyi veri tabanı modelina uyarlıyor.
            await _categoryCollection.InsertOneAsync(value);
            return value.CategoryId;
        }

        public async Task DeleteCategoryAsync(string id)
        {
            // Kullanılan kategori silinirse ürünler var olmayan bir kategoriyi gösterirdi (marka silmedeki kural).
            var productCount = await _productRepository.CountByCategoryIdAsync(id);
            if (productCount > 0)
                throw new CatalogDomainException($"Bu kategoride {productCount} ürün var; önce bu ürünlerin kategorisini değiştirin.");

            await _categoryCollection.DeleteOneAsync(x => x.CategoryId == id);
        }

        public async Task<List<ResultCategoryDto>> GetAllCategoryAsync()
        {
            var values = await _categoryCollection.Find(x => true).ToListAsync();

            return _mapper.Map<List<ResultCategoryDto>>(values);
        }

        public async Task<GetByIdCategoryDto> GetByIdCategoryAsync(string id)
        {
            var values = await _categoryCollection.Find(x => x.CategoryId == id).FirstOrDefaultAsync()
                ?? throw new NotFoundException("Kategori", id);
            return _mapper.Map<GetByIdCategoryDto>(values);
        }

        public async Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            var values = _mapper.Map<Category>(updateCategoryDto);
            await _categoryCollection.FindOneAndReplaceAsync(x => x.CategoryId == updateCategoryDto.CategoryId, values);
        }
    }
}
