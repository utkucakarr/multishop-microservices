using MultiShop.DtoLayer.CatalogDtos.BrandDtos;

namespace MultiShop.WebUI.Services.CatalogServices.BrandServices
{
    public class BrandService : IBrandService
    {
        private readonly HttpClient _httpClient;

        public BrandService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateBrandAsync(CreateBrandDto createBrandDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("brands", createBrandDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteBrandAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("brands?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultBrandDto>> GetAllBrandAsync()
        {
            var responseMessage = await _httpClient.GetAsync("brands");
            return await responseMessage.ReadOrThrowAsync<List<ResultBrandDto>>();
        }

        public async Task<UpdateBrandDto> GetByIdBrandAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("brands/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateBrandDto>();
        }

        public async Task<string?> UpdateBrandAsync(UpdateBrandDto updateBrandDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("brands", updateBrandDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
