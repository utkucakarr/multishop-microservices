using AutoMapper;
using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Exceptions;
using MultiShop.Catalog.Extensions;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public ProductService(IMapper mapper, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task CreateProductAsync(CreateProductDto createProductDto)
        {
            // ✅ Manual mapping — Factory Method üzerinden geçiyor
            var product = createProductDto.ToEntity();
            await _productRepository.CreateAsync(product);
        }

        public async Task DeleteProductAsync(string id)
            => await _productRepository.DeleteAsync(id);

        public async Task<IEnumerable<ResultProductDto>> GetAllProductAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ResultProductDto>>(products);
        }

        public async Task<GetByIdProductDto> GetByIdProductAsync(string id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<GetByIdProductDto?>(product);
        }

        public async Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryAsync()
        {
            // N+1 problemi C# tarafında In-Memory Join (Dictionary) kullanılarak çözüldü.
            var products = await _productRepository.GetProductsWithCategoryAsync();
            return _mapper.Map<IEnumerable<ResultProductsWithCategoryDto>>(products);
        }

        public async Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryByCategoryIdAsync(string categoryId)
        {
            // N+1 problemi In-Memory Join yöntemiyle çözüldü.
            var products = await _productRepository.GetProductsWithCategoryByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<ResultProductsWithCategoryDto>>(products);
        }

        public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            // Önce entity çekiliyor, sonra domain metodları ile güncelleniyor
            var product = await _productRepository.GetByIdAsync(updateProductDto.ProductId);
            if (product is null)
                throw new CatalogDomainException($"Product not found: {updateProductDto.ProductId}");

            product.UpdateCoreDetails(
                updateProductDto.ProductName,
                updateProductDto.ProductPrice,
                updateProductDto.CategoryId
            );
            product.UpdateProductDetails(
                updateProductDto.ThumbnailUrl
            );

            await _productRepository.UpdateAsync(product.ProductId, product);
        }
    }
}
