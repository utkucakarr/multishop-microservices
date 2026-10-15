using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Entities.ValueObject;
using MultiShop.Catalog.Exceptions;

namespace MultiShop.Catalog.UnitTests.Entities
{
    public class ProductDetailTests
    {
        private static ProductDetail CreateDetail()
            => ProductDetail.Create(new ProductDetailCreationParameters("6710f0a1b2c3d4e5f6a7b8c9", "Kısa", "Uzun açıklama", "Bilgi"));

        [Fact]
        public void Create_sets_fields()
        {
            var detail = CreateDetail();

            detail.ProductDetailId.Should().NotBeNullOrEmpty();
            detail.ProductId.Should().Be("6710f0a1b2c3d4e5f6a7b8c9");
            detail.ShortDescription.Should().Be("Kısa");
            detail.LongDescription.Should().Be("Uzun açıklama");
            detail.ProductInfo.Should().Be("Bilgi");
        }

        [Theory]
        [InlineData("", "Kısa")]
        [InlineData("6710f0a1b2c3d4e5f6a7b8c9", " ")]
        public void Creation_requires_product_and_short_description(string productId, string shortDescription)
        {
            var create = () => new ProductDetailCreationParameters(productId, shortDescription, "Uzun", "Bilgi");

            create.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void UpdateDetails_replaces_all_texts()
        {
            var detail = CreateDetail();

            detail.UpdateDetails("Yeni kısa", "Yeni uzun", "Yeni bilgi");

            detail.ShortDescription.Should().Be("Yeni kısa");
            detail.LongDescription.Should().Be("Yeni uzun");
            detail.ProductInfo.Should().Be("Yeni bilgi");
        }

        [Fact]
        public void UpdateDetails_rejects_empty_short_description_and_keeps_old_texts()
        {
            var detail = CreateDetail();

            var update = () => detail.UpdateDetails("", "Yeni uzun", "Yeni bilgi");

            update.Should().Throw<CatalogDomainException>();
            detail.ShortDescription.Should().Be("Kısa");
            detail.LongDescription.Should().Be("Uzun açıklama");
        }
    }
}
