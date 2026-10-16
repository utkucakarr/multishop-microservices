using MultiShop.DtoLayer.CatalogDtos.SpecialOfferDtos;

namespace MultiShop.WebUI.Services.CatalogServices.SpecialOfferServices
{
    public class SpecialOfferService : ISpecialOfferService
    {
        private readonly HttpClient _httpClient;

        public SpecialOfferService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateSpecialOfferAsync(CreateSpecialOfferDto createSpecialOfferDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("specialoffers", createSpecialOfferDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteSpecialOfferAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("specialoffers?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultSpecialOfferDto>> GetAllSpecialOfferAsync()
        {
            var responseMessage = await _httpClient.GetAsync("specialoffers");
            return await responseMessage.ReadOrThrowAsync<List<ResultSpecialOfferDto>>();
        }

        public async Task<UpdateSpecialOfferDto> GetByIdSpecialOfferAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("specialoffers/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateSpecialOfferDto>();
        }

        public async Task<string?> UpdateSpecialOfferAsync(UpdateSpecialOfferDto updateSpecialOfferDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("specialoffers", updateSpecialOfferDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
