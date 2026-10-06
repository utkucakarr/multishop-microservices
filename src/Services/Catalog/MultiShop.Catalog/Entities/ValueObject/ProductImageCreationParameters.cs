using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities.ValueObject
{
    public record ProductImageCreationParameters
    {
        public string ProductId { get; init; }
        public string ImageUrl { get; init; }
        public int DisplayOrder { get; init; }
        public bool IsMain { get; init; }

        public ProductImageCreationParameters(
            string productId,
            string imageUrl,
            int displayOrder,
            bool isMain)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new CatalogDomainException("ProductId cannot be empty.");
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new CatalogDomainException("ImageUrl cannot be empty.");
            if (displayOrder < 1)
                throw new CatalogDomainException("DisplayOrder must be greater than zero.");

            ProductId = productId;
            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;
            IsMain = isMain;
        }
    }
}
