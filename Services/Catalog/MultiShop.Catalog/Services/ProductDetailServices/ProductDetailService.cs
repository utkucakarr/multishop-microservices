using AutoMapper;
using MongoDB.Driver;
using MultiShop.Catalog.Dtos.ProductDetailDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Exceptions;
using MultiShop.Catalog.Extensions;
using MultiShop.Catalog.Repositories.Interfaces;

namespace MultiShop.Catalog.Services.ProductDetailServices
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly IMapper _mapper;
        private readonly IProductDetailRepository _repository;

        public ProductDetailService(IProductDetailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto)
        {
            var productDetail = createProductDetailDto.ToEntity();
            await _repository.CreateAsync(productDetail);
        }

        public async Task DeleteProductDetailAsync(string id)
            => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<ResultProductDetailDto>> GetAllProductDetailAsync()
        {
            var values = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ResultProductDetailDto>>(values);
        }

        public async Task<GetByIdProductDetailDto> GetByIdProductDetailAsync(string id)
        {
            var productDetail = await _repository.GetByIdAsync(id);
            return _mapper.Map<GetByIdProductDetailDto?>(productDetail);
        }

        public async Task<GetByIdProductDetailDto> GetByProductIdProductDetailAsync(string id)
        {
            var productDetail = await _repository.GetByProductIdAsync(id);
            return _mapper.Map<GetByIdProductDetailDto?>(productDetail);
        }

        public async Task UpdateProductDetailAsyn(UpdateProductDetailDto updateProductDetailDto)
        {
            var productDetail = await _repository
                .GetByIdAsync(updateProductDetailDto.ProductDetailId);

            if (productDetail is null)
                throw new CatalogDomainException(
                    $"ProductDetail not found: {updateProductDetailDto.ProductDetailId}"
                );

            productDetail.UpdateDetails(
                updateProductDetailDto.ShortDescription,
                updateProductDetailDto.LongDescription,
                updateProductDetailDto.ProductInfo
            );

            await _repository.UpdateAsync(
                productDetail.ProductDetailId,
                productDetail
            );
        }
    }
}
