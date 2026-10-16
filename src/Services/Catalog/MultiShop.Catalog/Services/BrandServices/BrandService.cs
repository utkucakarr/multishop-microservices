using AutoMapper;
using MongoDB.Driver;
using MultiShop.Catalog.Dtos.BrandDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Exceptions;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Catalog.Services.BrandServices
{
    public class BrandService : IBrandService
    {
        private readonly IMongoCollection<Brand> _brandCollection;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public BrandService(IMapper mapper, IMongoDatabase database, IDatabaseSettings _databaseSettings, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _brandCollection = database.GetCollection<Brand>(_databaseSettings.BrandCollectionName);
            _mapper = mapper;
        }

        public async Task<string> CreateBrandAsync(CreateBrandDto createBrandDto)
        {
            var values = _mapper.Map<Brand>(createBrandDto);
            await _brandCollection.InsertOneAsync(values);
            return values.BrandId;
        }

        public async Task DeleteBrandAsync(string id)
        {
            // Kullanılan marka silinirse ürünler var olmayan bir markayı gösterirdi.
            var productCount = await _productRepository.CountByBrandIdAsync(id);
            if (productCount > 0)
                throw new CatalogDomainException($"Bu markaya bağlı {productCount} ürün var; önce bu ürünlerin markasını değiştirin.");

            await _brandCollection.DeleteOneAsync(x => x.BrandId == id);
        }

        public async Task<List<ResultBrandDto>> GetAllBrandAsync()
        {
            var values = await _brandCollection.Find(x => true).ToListAsync();
            return _mapper.Map<List<ResultBrandDto>>(values);
        }

        public async Task<GetByIdBrandDto> GetByIdBrandAsync(string id)
        {
            var values = await _brandCollection.Find(x => x.BrandId == id).FirstOrDefaultAsync()
                ?? throw new NotFoundException("Marka", id);
            return _mapper.Map<GetByIdBrandDto>(values);
        }

        public async Task UpdateBrandAsync(UpdateBrandDto updateBrandDto)
        {
            var values = _mapper.Map<Brand>(updateBrandDto);
            await _brandCollection.FindOneAndReplaceAsync(x => x.BrandId == updateBrandDto.BrandId, values);
        }
    }
}
