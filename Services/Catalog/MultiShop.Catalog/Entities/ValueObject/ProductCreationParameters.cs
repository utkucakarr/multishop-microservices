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
                throw new ArgumentException("Product name cannot be empty.", nameof(productName));
            if (productPrice <= 0)
                throw new ArgumentException("Product price must be greater than zero.", nameof(productPrice));
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));
            if (initialStock < 0)
                throw new ArgumentException("Initial stock cannot be negative.", nameof(initialStock));
            if (restockThreshold < 0)
                throw new ArgumentException("Restock threshold cannot be negative.", nameof(restockThreshold));
            if (maxStockThreshold <= restockThreshold)
                throw new ArgumentException("Max stock threshold must be greater than restock threshold.", nameof(maxStockThreshold));

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
