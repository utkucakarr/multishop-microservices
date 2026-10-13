using MultiShop.DtoLayer.CatalogDtos.ProductImageDtos;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MultiShop.WebUI.Services.CatalogServices.ProductImageServices
{
    public class ProductImageService : IProductImageService
    {
        private readonly HttpClient _httpClient;

        public ProductImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateProductImageAsync(CreateProductImageDto createProductImageDto)
        {
            await _httpClient.PostAsJsonAsync<CreateProductImageDto>("productimage", createProductImageDto);
        }

        public async Task<string?> UploadProductImageAsync(string productId, IFormFile file, bool isMain)
        {
            await using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            if (!string.IsNullOrEmpty(file.ContentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(productId), "ProductId" },
                { new StringContent(isMain.ToString()), "IsMain" },
                { fileContent, "File", file.FileName }
            };

            var responseMessage = await _httpClient.PostAsync("productimage/upload", content);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> SetMainProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.PutAsync($"productimage/{id}/main", null);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("productimage?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultProductImageDto>> GetAllProductImageAsync()
        {
            var responseMessage = await _httpClient.GetAsync("productimage");
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultProductImageDto>>(jsonData);
            return values;
        }

        public async Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productimage/" + id);
            var values = await responseMessage.Content.ReadFromJsonAsync<GetByIdProductImageDto>();
            return values;
        }

        public async Task<List<GetByIdProductImageDto>> GetByProductIdProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productimage/ProductImagesByProductId/" + id);
            if (!responseMessage.IsSuccessStatusCode)
                return new List<GetByIdProductImageDto>();

            var values = await responseMessage.Content.ReadFromJsonAsync<List<GetByIdProductImageDto>>();
            return values ?? new List<GetByIdProductImageDto>();
        }

        public async Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateProductImageDto>("productimage", updateProductImageDto);
        }
    }
}
