using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities
{
    public class ProductDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductDetailId { get; private set; }
        public string ProductId { get; private set; }
        public string ShortDescription { get; private set; }
        public string LongDescription { get; private set; }
        public string ProductInfo { get; private set; }

        [BsonIgnore]
        public Product Product { get; set; }

        [BsonConstructor]
        private ProductDetail() { }

        public static ProductDetail Create(ProductDetailCreationParameters parameters)
        {
            return new ProductDetail
            {
                ProductDetailId = ObjectId.GenerateNewId().ToString(),
                ProductId = parameters.ProductId,
                ShortDescription = parameters.ShortDescription,
                LongDescription = parameters.LongDescription,
                ProductInfo = parameters.ProductInfo
            };
        }

        public void UpdateDetails(string shortDescription, string longDescription, string productInfo)
        {
            if (string.IsNullOrWhiteSpace(shortDescription))
                throw new CatalogDomainException("Short description cannot be empty.");

            ShortDescription = shortDescription;
            LongDescription = longDescription;
            ProductInfo = productInfo;
        }
    }
}