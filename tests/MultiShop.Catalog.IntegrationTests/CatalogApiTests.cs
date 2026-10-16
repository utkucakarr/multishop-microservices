using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MultiShop.Catalog.Dtos.ProductDetailDtos;
using MultiShop.Catalog.IntegrationTests.Infrastructure;

namespace MultiShop.Catalog.IntegrationTests
{
    /// <summary>Catalog uçları: yetkilendirme, yanıt kodları (T-2) ve silme bütünlüğü (W-6). Docker gerekir.</summary>
    [Collection(CatalogApiCollection.Name)]
    public class CatalogApiTests
    {
        private readonly CatalogApiFactory _factory;
        private readonly HttpClient _admin;

        public CatalogApiTests(CatalogApiFactory factory)
        {
            _factory = factory;
            _admin = factory.CreateClientAs(TestUser.Admin);
        }

        [Theory]
        [InlineData(TestUser.Anonymous, HttpStatusCode.Unauthorized)]
        [InlineData(TestUser.Visitor, HttpStatusCode.OK)]
        [InlineData(TestUser.Customer, HttpStatusCode.OK)]
        public async Task Reading_categories_requires_a_token(TestUser user, HttpStatusCode expected)
        {
            var response = await _factory.CreateClientAs(user).GetAsync("api/categories");

            response.StatusCode.Should().Be(expected);
        }

        [Theory]
        [InlineData(TestUser.Anonymous, HttpStatusCode.Unauthorized)]
        [InlineData(TestUser.Visitor, HttpStatusCode.Forbidden)]
        [InlineData(TestUser.Customer, HttpStatusCode.Forbidden)]
        public async Task Only_admin_can_create_a_category(TestUser user, HttpStatusCode expected)
        {
            var response = await _factory.CreateClientAs(user).PostAsJsonAsync("api/categories", new { CategoryName = "Yetkisiz", ImageUrl = "x" });

            response.StatusCode.Should().Be(expected);
        }

        [Fact]
        public async Task Created_category_returns_201_with_its_location()
        {
            var response = await _admin.PostAsJsonAsync("api/categories", new { CategoryName = "Ayakkabı", ImageUrl = "/img/a.png" });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = (await response.Content.ReadFromJsonAsync<CatalogClientExtensions.CreatedId>())!.Id;
            response.Headers.Location!.ToString().Should().EndWith($"api/Categories/{id}");
            (await _admin.GetAsync(response.Headers.Location)).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Unknown_product_returns_404_and_malformed_id_returns_400()
        {
            (await _admin.GetAsync($"api/products/{ObjectId.GenerateNewId()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await _admin.GetAsync("api/products/gecersiz-id")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Product_cannot_be_created_in_a_missing_category()
        {
            var response = await _admin.PostAsJsonAsync("api/products", new
            {
                ProductName = "Kazak", ProductPrice = 100m, CategoryId = ObjectId.GenerateNewId().ToString(),
                AvailableStock = 1, RestockThreshold = 1, MaxStockThreshold = 10
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().StartWith("Kategori bulunamadı");
        }

        [Fact]
        public async Task Category_with_products_cannot_be_deleted()
        {
            var categoryId = await _admin.CreateCategoryAsync();
            await _admin.CreateProductAsync(categoryId);

            var response = await _admin.DeleteAsync($"api/categories?id={categoryId}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Contain("Bu kategoride 1 ürün var");
            (await _admin.GetAsync($"api/categories/{categoryId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Empty_category_is_deleted()
        {
            var categoryId = await _admin.CreateCategoryAsync();

            (await _admin.DeleteAsync($"api/categories?id={categoryId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await _admin.GetAsync($"api/categories/{categoryId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Deleting_a_product_also_deletes_its_detail()
        {
            var productId = await _admin.CreateProductAsync(await _admin.CreateCategoryAsync());
            var detail = await _admin.PostAsJsonAsync("api/productdetails", new CreateProductDetailDto
            {
                ProductId = productId, ShortDescription = "Kısa", LongDescription = "Uzun", ProductInfo = "Bilgi"
            });
            detail.StatusCode.Should().Be(HttpStatusCode.Created);

            (await _admin.DeleteAsync($"api/products?id={productId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

            (await _admin.GetAsync($"api/products/{productId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await _admin.GetAsync($"api/productdetails/GetProductDetailByProductId/{productId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
