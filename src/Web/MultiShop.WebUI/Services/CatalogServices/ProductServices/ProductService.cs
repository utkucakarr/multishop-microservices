using MultiShop.DtoLayer.CatalogDtos.ProductDtos;

namespace MultiShop.WebUI.Services.CatalogServices.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateProductAsync(CreateProductDto createProductDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync<CreateProductDto>("products", createProductDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteProductAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("products?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultProductDto>> GetAllProductAsync()
        {
            var responseMessage = await _httpClient.GetAsync("products");
            return await responseMessage.ReadOrThrowAsync<List<ResultProductDto>>();
        }

        public async Task<GetByIdProductDto> GetByIdProductAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("products/" + id);
            return await responseMessage.ReadOrThrowAsync<GetByIdProductDto>();
        }

        public async Task<List<ResultProductWithCategoryDto>> GetProductsWithCategoryByCategoryIdAsync(string CategoryId)
        {
            var responseMessage = await _httpClient.GetAsync("products/GetProductsWithCategoryByCategoryId/" + CategoryId);
            return await responseMessage.ReadOrThrowAsync<List<ResultProductWithCategoryDto>>();
        }

        public async Task<string?> UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync<UpdateProductDto>("products", updateProductDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> UpdateStockAsync(string productId, int availableStock)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync($"products/{Uri.EscapeDataString(productId)}/stock", new { availableStock });
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
