using MongoDB.Bson;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.UnitTests.Entities
{
    public class ProductTests
    {
        private static ProductCreationParameters ValidParameters(
            string productName = "Kazak",
            decimal productPrice = 199.90m,
            string categoryId = "6710f0a1b2c3d4e5f6a7b8c9",
            int initialStock = 10,
            int restockThreshold = 5,
            int maxStockThreshold = 100,
            string? brandId = null)
            => new(productName, productPrice, categoryId, initialStock, restockThreshold, maxStockThreshold,
                productDescription: "Yün kazak", productImageUrl: "/uploads/kazak.jpg", brandId: brandId);

        [Fact]
        public void Create_sets_fields_and_starts_with_no_reserved_stock()
        {
            var product = Product.Create(ValidParameters());

            ObjectId.TryParse(product.ProductId, out _).Should().BeTrue("Mongo kimliği uygulamada üretilir");
            product.ProductName.Should().Be("Kazak");
            product.ProductPrice.Should().Be(199.90m);
            product.AvailableStock.Should().Be(10);
            product.ReservedStock.Should().Be(0);
            product.SellableStock.Should().Be(10);
            product.OnReorder.Should().BeFalse();
        }

        [Fact]
        public void Create_generates_a_different_id_for_each_product()
        {
            Product.Create(ValidParameters()).ProductId.Should().NotBe(Product.Create(ValidParameters()).ProductId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Blank_brand_is_stored_as_no_brand(string? brandId)
        {
            Product.Create(ValidParameters(brandId: brandId)).BrandId.Should().BeNull();
        }

        public static TheoryData<string, Func<ProductCreationParameters>> InvalidParameters => new()
        {
            { "boş ad", () => ValidParameters(productName: " ") },
            { "sıfır fiyat", () => ValidParameters(productPrice: 0) },
            { "negatif fiyat", () => ValidParameters(productPrice: -1) },
            { "boş kategori", () => ValidParameters(categoryId: "") },
            { "negatif başlangıç stoğu", () => ValidParameters(initialStock: -1) },
            { "negatif yeniden sipariş eşiği", () => ValidParameters(restockThreshold: -1) },
            { "üst eşik alt eşikten büyük değil", () => ValidParameters(restockThreshold: 10, maxStockThreshold: 10) },
        };

        [Theory]
        [MemberData(nameof(InvalidParameters))]
        public void Creation_parameters_reject_invalid_values(string because, Func<ProductCreationParameters> create)
        {
            create.Should().Throw<CatalogDomainException>(because);
        }

        [Fact]
        public void UpdateCoreDetails_changes_name_price_category_and_brand()
        {
            var product = Product.Create(ValidParameters());

            product.UpdateCoreDetails("Hırka", 249.50m, "6710f0a1b2c3d4e5f6a7b8ca", "6710f0a1b2c3d4e5f6a7b8cb");

            product.ProductName.Should().Be("Hırka");
            product.ProductPrice.Should().Be(249.50m);
            product.CategoryId.Should().Be("6710f0a1b2c3d4e5f6a7b8ca");
            product.BrandId.Should().Be("6710f0a1b2c3d4e5f6a7b8cb");
        }

        [Theory]
        [InlineData("", 10, "cat")]
        [InlineData("Hırka", 0, "cat")]
        [InlineData("Hırka", 10, " ")]
        public void UpdateCoreDetails_rejects_invalid_values_and_keeps_old_ones(string name, decimal price, string categoryId)
        {
            var product = Product.Create(ValidParameters());

            var update = () => product.UpdateCoreDetails(name, price, categoryId, brandId: null);

            update.Should().Throw<CatalogDomainException>();
            product.ProductName.Should().Be("Kazak");
            product.ProductPrice.Should().Be(199.90m);
        }

        [Fact]
        public void UpdateCoreDetails_with_blank_brand_removes_the_brand()
        {
            var product = Product.Create(ValidParameters(brandId: "6710f0a1b2c3d4e5f6a7b8cb"));

            product.UpdateCoreDetails("Kazak", 199.90m, "6710f0a1b2c3d4e5f6a7b8c9", " ");

            product.BrandId.Should().BeNull();
        }
    }
}
