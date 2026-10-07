using AutoMapper;
using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Extensions;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Settings;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Catalog.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;

        public ProductService(IMapper mapper, IProductRepository productRepository, IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
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
            var dtos = _mapper.Map<List<ResultProductDto>>(products);
            await ApplyMainImagesAsync(dtos, x => x.ProductId, (x, url) => x.ProductImageUrl = url);
            return dtos;
        }

        public async Task<GetByIdProductDto> GetByIdProductAsync(string id)
        {
            var product = await _productRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Ürün", id);
            var dto = _mapper.Map<GetByIdProductDto>(product);
            await ApplyMainImagesAsync(new List<GetByIdProductDto> { dto }, x => x.ProductId, (x, url) => x.ProductImageUrl = url);
            return dto;
        }

        public async Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryAsync()
        {
            // N+1 problemi C# tarafında In-Memory Join (Dictionary) kullanılarak çözüldü.
            var products = await _productRepository.GetProductsWithCategoryAsync();
            var dtos = _mapper.Map<List<ResultProductsWithCategoryDto>>(products);
            await ApplyMainImagesAsync(dtos, x => x.ProductId, (x, url) => x.ProductImageUrl = url);
            return dtos;
        }

        public async Task<IEnumerable<ResultProductsWithCategoryDto>> GetProductsWithCategoryByCategoryIdAsync(string categoryId)
        {
            // N+1 problemi In-Memory Join yöntemiyle çözüldü.
            var products = await _productRepository.GetProductsWithCategoryByCategoryIdAsync(categoryId);
            var dtos = _mapper.Map<List<ResultProductsWithCategoryDto>>(products);
            await ApplyMainImagesAsync(dtos, x => x.ProductId, (x, url) => x.ProductImageUrl = url);
            return dtos;
        }

        // Ürün görselinin tek kaynağı ProductImage koleksiyonundaki ana (IsMain) görseldir.
        // Ana görseli olmayan ürünlerde Product.ProductImageUrl olduğu gibi kalır.
        private async Task ApplyMainImagesAsync<TDto>(List<TDto> dtos, Func<TDto, string> getProductId, Action<TDto, string> setImageUrl)
        {
            if (dtos.Count == 0)
                return;

            var mainImageUrls = await _productImageRepository.GetMainImageUrlsAsync(dtos.Select(getProductId));
            foreach (var dto in dtos)
            {
                if (mainImageUrls.TryGetValue(getProductId(dto), out var imageUrl))
                    setImageUrl(dto, imageUrl);
            }
        }

        public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            // Önce entity çekiliyor, sonra domain metodları ile güncelleniyor
            var product = await _productRepository.GetByIdAsync(updateProductDto.ProductId);
            if (product is null)
                throw new NotFoundException("Ürün", updateProductDto.ProductId);

            product.UpdateCoreDetails(
                updateProductDto.ProductName,
                updateProductDto.ProductPrice,
                updateProductDto.CategoryId
            );
            product.UpdateProductDetails(
                updateProductDto.ProductDescription,
                updateProductDto.ProductImageUrl
            );

            await _productRepository.UpdateAsync(product.ProductId, product);
        }
    }
}
