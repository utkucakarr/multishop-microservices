using MultiShop.DtoLayer.CatalogDtos.AboutDtos;

namespace MultiShop.WebUI.Services.CatalogServices.AboutServices
{
    public class AboutService : IAboutService
    {
        private readonly HttpClient _httpClient;

        public AboutService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateAboutAsync(CreateAboutDto createAboutDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("abouts", createAboutDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteAboutAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("abouts?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultAboutDto>> GetAllAboutAsync()
        {
            var responseMessage = await _httpClient.GetAsync("abouts");
            return await responseMessage.ReadOrThrowAsync<List<ResultAboutDto>>();
        }

        public async Task<UpdateAboutDto> GetByIdAboutAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("abouts/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateAboutDto>();
        }

        public async Task<string?> UpdateAboutAsync(UpdateAboutDto updateAboutDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("abouts", updateAboutDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
