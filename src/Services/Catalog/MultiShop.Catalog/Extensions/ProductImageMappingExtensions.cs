using MultiShop.Catalog.Dtos.ProductImageDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;

namespace MultiShop.Catalog.Extensions
{
    public static class ProductImageMappingExtensions
    {
        public static ProductImage ToEntity(this CreateProductImageDto dto)
        {
            var parameters = new ProductImageCreationParameters(
                productId: dto.ProductId,
                imageUrl: dto.ImageUrl,
                displayOrder: dto.DisplayOrder,
                isMain: dto.IsMain
            );

            return ProductImage.Create(parameters);
        }
    }
}
