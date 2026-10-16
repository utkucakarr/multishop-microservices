using MultiShop.DtoLayer.CatalogDtos.FeatureSliderDtos;

namespace MultiShop.WebUI.Services.CatalogServices.FeatureSliderServices
{
    public class FeatureSliderService : IFeatureSliderService
    {
        private readonly HttpClient _httpClient;

        public FeatureSliderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateFeatureSliderAsync(CreateFeatureSliderDto createFeatureSliderDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("featuresliders", createFeatureSliderDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteFeatureSliderAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("featuresliders?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultFeatureSliderDto>> GetAllFeatureSliderAsync()
        {
            var responseMessage = await _httpClient.GetAsync("featuresliders");
            return await responseMessage.ReadOrThrowAsync<List<ResultFeatureSliderDto>>();
        }

        public async Task<UpdateFeatureSliderDto> GetByIdFeatureSliderAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("featuresliders/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateFeatureSliderDto>();
        }

        public async Task<string?> UpdateFeatureSliderAsync(UpdateFeatureSliderDto updateFeatureSliderDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("featuresliders", updateFeatureSliderDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
