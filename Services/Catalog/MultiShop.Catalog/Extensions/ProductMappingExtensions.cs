using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;

namespace MultiShop.Catalog.Extensions
{
    public static class ProductMappingExtensions
    {
        public static Product ToEntity(this CreateProductDto dto)
        {
            var parameters = new ProductCreationParameters(
                productName: dto.ProductName,
                productPrice: dto.ProductPrice,
                categoryId: dto.CategoryId,
                initialStock: dto.AvailableStock,
                restockThreshold: dto.RestockThreshold,
                maxStockThreshold: dto.MaxStockThreshold,
                productDescription: dto.ProductDescription,
                productImageUrl: dto.ProductImageUrl
            );

            return Product.Create(parameters);
        }
    }
}