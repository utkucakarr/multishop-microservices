using AutoMapper;
using MongoDB.Driver;
using MultiShop.Catalog.Dtos.ProductImageDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Exceptions;
using MultiShop.Catalog.Extensions;
using MultiShop.Catalog.Repositories.Interfaces;

namespace MultiShop.Catalog.Services.ProductImageServices
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IMapper _mapper;

        public ProductImageService(IProductImageRepository productImageRepository,
            IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _mapper = mapper;
        }
        public async Task CreateProductImageAsync(CreateProductImageDto createProductImageDto)
        {
            var productImage = createProductImageDto.ToEntity();
            await _productImageRepository.CreateAsync(productImage);
        }

        public async Task DeleteProductImageAsync(string id)
            => await _productImageRepository.DeleteAsync(id);

        public async Task<IEnumerable<ResultProductImageDto>> GetAllProductImageAsync()
        {
            var values = await _productImageRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ResultProductImageDto>>(values);
        }

        public async Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id)
        {
            var productImage = await _productImageRepository.GetByIdAsync(id);
            return _mapper.Map<GetByIdProductImageDto?>(productImage);
        }

        public async Task<IEnumerable<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id)
        {
            var productImages = await _productImageRepository
                           .GetImagesByProductIdAsync(id);

            return _mapper.Map<IEnumerable<GetByIdProductImageDto>>(productImages);
        }

        public async Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto)
        {
            var productImage = await _productImageRepository
                            .GetByIdAsync(updateProductImageDto.ProductImageId);

            if (productImage is null)
                throw new CatalogDomainException(
                    $"ProductImage not found: {updateProductImageDto.ProductImageId}"
                );

            productImage.UpdateImage(
                updateProductImageDto.ImageUrl,
                updateProductImageDto.DisplayOrder,
                updateProductImageDto.IsMain
            );

            await _productImageRepository.UpdateAsync(
                productImage.ProductImageId,
                productImage
            );
        }
    }
}
