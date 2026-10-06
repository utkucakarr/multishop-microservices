using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities
{
    public class ProductImage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductImageId { get; private set; }
        public string ProductId { get; private set; }
        public string ImageUrl { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsMain { get; private set; }

        [BsonIgnore]
        public Product Product { get; set; }

        [BsonConstructor]
        private ProductImage() { }

        public static ProductImage Create(ProductImageCreationParameters parameters)
        {
            return new ProductImage
            {
                ProductImageId = ObjectId.GenerateNewId().ToString(),
                ProductId = parameters.ProductId,
                ImageUrl = parameters.ImageUrl,
                DisplayOrder = parameters.DisplayOrder,
                IsMain = parameters.IsMain
            };
        }

        public void UpdateImage(string imageUrl, int displayOrder, bool isMain)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new CatalogDomainException("ImageUrl cannot be empty.");
            if (displayOrder < 1)
                throw new CatalogDomainException("DisplayOrder must be greater than zero.");

            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;
            IsMain = isMain;
        }
    }
}
