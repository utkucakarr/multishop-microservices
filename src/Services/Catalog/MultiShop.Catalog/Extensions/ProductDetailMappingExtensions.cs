using MultiShop.Catalog.Dtos.ProductDetailDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;

namespace MultiShop.Catalog.Extensions
{
    public static class ProductDetailMappingExtensions
    {
        public static ProductDetail ToEntity(this CreateProductDetailDto dto)
        {
            var parameters = new ProductDetailCreationParameters(
                productId: dto.ProductId,
                shortDescription: dto.ShortDescription,
                longDescription: dto.LongDescription,
                productInfo: dto.ProductInfo
            );

            return ProductDetail.Create(parameters);
        }
    }
}
