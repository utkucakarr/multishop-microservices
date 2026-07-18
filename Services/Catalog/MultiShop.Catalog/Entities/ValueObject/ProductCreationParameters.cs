using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities.ValueObject
{
    public record ProductCreationParameters
    {
        public string ProductName { get; init; }
        public decimal ProductPrice { get; init; }
        public string CategoryId { get; init; }
        public int InitialStock { get; init; }
        public int RestockThreshold { get; init; }
        public int MaxStockThreshold { get; init; }
        public string? ProductDescription { get; init; }
        public string? ProductImageUrl { get; init; }

        // Constructor içinde validasyon — domain kuralları burada da korunur
        public ProductCreationParameters(
            string productName,
            decimal productPrice,
            string categoryId,
            int initialStock,
            int restockThreshold,
            int maxStockThreshold,
            string? productDescription = null,
            string? productImageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new CatalogDomainException("Product name cannot be empty.");
            if (productPrice <= 0)
                throw new CatalogDomainException("Product price must be greater than zero.");
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new CatalogDomainException("Category ID cannot be empty.");
            if (initialStock < 0)
                throw new CatalogDomainException("Initial stock cannot be negative.");
            if (restockThreshold < 0)
                throw new CatalogDomainException("Restock threshold cannot be negative.");
            if (maxStockThreshold <= restockThreshold)
                throw new CatalogDomainException("Max stock threshold must be greater than restock threshold.");

            ProductName = productName;
            ProductPrice = productPrice;
            CategoryId = categoryId;
            InitialStock = initialStock;
            RestockThreshold = restockThreshold;
            MaxStockThreshold = maxStockThreshold;
            ProductDescription = productDescription;
            ProductImageUrl = productImageUrl;
        }
    }
}
