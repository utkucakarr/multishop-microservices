using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.Entities
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; private set; }

        public string ProductName { get; private set; }

        public decimal ProductPrice { get; private set; }

        public string? ProductImageUrl { get; private set; }

        public string? ProductDescription { get; private set; }

        public string CategoryId { get; private set; }

        /// <summary>İsteğe bağlı; markası olmayan ürünlerde null.</summary>
        public string? BrandId { get; private set; }

        // --- STOK YÖNETİMİ ALANLARI ---
        // Set işlemi dışarıya kapatıldı (private set)
        public int AvailableStock { get; private set; }

        public int RestockThreshold { get; private set; } // Minimum güvenli stok seviyesi

        public int MaxStockThreshold { get; private set; } // Depoda tutulacak maksimum stok miktarıdır.

        // Eşik alanları ürün oluşturulurken kaydedilir; "stok eşiğin altında" uyarısı henüz yok.
        // Stok değişiklikleri: StockReservationService (ayır/düş/bırak) ve admin stok güncellemesi.
        public bool OnReorder { get; private set; }

        /// <summary>
        /// Ödemesi beklenen siparişlere ayrılmış adet (KT-12). <see cref="AvailableStock"/> eldeki stoktur;
        /// ödeme gelince ikisi birlikte düşer, sipariş iptalinde yalnızca bu azalır. Yalnızca stok rezervasyonu
        /// (StockReservationService) atomik $inc ile değiştirir; eski dokümanlarda alan yoktur, 0 sayılır.
        /// </summary>
        public int ReservedStock { get; private set; }

        /// <summary>Satılabilir adet: eldeki − ayrılmış.</summary>
        [BsonIgnore]
        public int SellableStock => AvailableStock - ReservedStock;

        [BsonIgnore]
        public Category Category { get; set; }

        [BsonIgnore]
        public Brand? Brand { get; set; }

        // MongoDB için parametresiz constructor (private — dışarıdan erişilemesin)
        private Product() { }

        public static Product Create(ProductCreationParameters parameters)
        {
            return new Product
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                ProductName = parameters.ProductName,
                ProductPrice = parameters.ProductPrice,
                CategoryId = parameters.CategoryId,
                BrandId = parameters.BrandId,
                AvailableStock = parameters.InitialStock,
                RestockThreshold = parameters.RestockThreshold,
                MaxStockThreshold = parameters.MaxStockThreshold,
                OnReorder = false,
                ProductDescription = parameters.ProductDescription,
                ProductImageUrl = parameters.ProductImageUrl
            };
        }

        public void UpdateProductDetails(string? description, string? imageUrl)
        {
            // İleride buraya kural ekleyebilirsin:
            // Örneğin "description boş olamaz" veya "imageUrl http ile başlamalı" gibi.

            ProductDescription = description;
            ProductImageUrl = imageUrl;
        }

        public void UpdateCoreDetails(string productName, decimal productPrice, string categoryId, string? brandId)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new CatalogDomainException("Product name cannot be empty.");
            if (productPrice <= 0)
                throw new CatalogDomainException("Product price must be greater than zero.");
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new CatalogDomainException("Category ID cannot be empty.");

            ProductName = productName;
            ProductPrice = productPrice;
            CategoryId = categoryId;
            BrandId = string.IsNullOrWhiteSpace(brandId) ? null : brandId;
        }
    }
}
