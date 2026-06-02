using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities.ValueObject
{
    public record ProductDetailCreationParameters
    {
        public string ProductId { get; init; }
        public string ShortDescription { get; init; }
        public string LongDescription { get; init; }
        public string ProductInfo { get; init; }

        public ProductDetailCreationParameters(
           string productId,
           string shortDescription,
           string longDescription,
           string productInfo)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new CatalogDomainException("ProductId cannot be empty.");
            if (string.IsNullOrWhiteSpace(shortDescription))
                throw new CatalogDomainException("Short description cannot be empty.");

            ProductId = productId;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            ProductInfo = productInfo;
        }
    }
}
