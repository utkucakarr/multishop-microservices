using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;

namespace MultiShop.WebUI.Services.CatalogServices.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;

        public CategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("categories", createCategoryDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteCategoryAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("categories?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultCategoryDto>> GetAllCategoryAsync()
        {
            var responseMessage = await _httpClient.GetAsync("categories");
            return await responseMessage.ReadOrThrowAsync<List<ResultCategoryDto>>();
        }

        public async Task<UpdateCategoryDto> GetByIdCategoryAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("categories/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateCategoryDto>();
        }

        public async Task<string?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("categories", updateCategoryDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
