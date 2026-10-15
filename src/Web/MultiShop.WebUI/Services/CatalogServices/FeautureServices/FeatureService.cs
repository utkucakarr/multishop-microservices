using MultiShop.DtoLayer.CatalogDtos.FeatureDtos;

namespace MultiShop.WebUI.Services.CatalogServices.FeautureServices
{
    public class FeatureService : IFeatureService
    {
        private readonly HttpClient _httpClient;

        public FeatureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateFeatureAsync(CreateFeatureDto createFeatureDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("features", createFeatureDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteFeatureAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("features?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultFeatureDto>> GetAllFeatureAsync()
        {
            var responseMessage = await _httpClient.GetAsync("features");
            return await responseMessage.ReadOrThrowAsync<List<ResultFeatureDto>>();
        }

        public async Task<UpdateFeatureDto> GetByIdFeatureAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("features/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateFeatureDto>();
        }

        public async Task<string?> UpdateFeatureAsync(UpdateFeatureDto updateFeatureDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("features", updateFeatureDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
