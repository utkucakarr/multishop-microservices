using System.Net;
using System.Net.Http.Json;
using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Dtos.ProductDtos;

namespace MultiShop.Catalog.IntegrationTests.Infrastructure
{
    /// <summary>Testlerin ortak kurulum adımları, admin istemcisiyle API üzerinden.</summary>
    public static class CatalogClientExtensions
    {
        public record CreatedId(string Id);

        public static async Task<string> CreateCategoryAsync(this HttpClient admin, string name = "Giyim")
        {
            var response = await admin.PostAsJsonAsync("api/categories", new CreateCategoryDto { CategoryName = name, ImageUrl = "/img/giyim.png" });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<CreatedId>())!.Id;
        }

        public static async Task<string> CreateProductAsync(this HttpClient admin, string categoryId, int stock = 10, string name = "Kazak")
        {
            var response = await admin.PostAsJsonAsync("api/products", new CreateProductDto
            {
                ProductName = name,
                ProductPrice = 199.90m,
                CategoryId = categoryId,
                AvailableStock = stock,
                RestockThreshold = 1,
                MaxStockThreshold = 1000
            });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<CreatedId>())!.Id;
        }

        public static async Task<GetByIdProductDto> GetProductAsync(this HttpClient client, string productId)
            => (await client.GetFromJsonAsync<GetByIdProductDto>($"api/products/{productId}"))!;
    }
}
