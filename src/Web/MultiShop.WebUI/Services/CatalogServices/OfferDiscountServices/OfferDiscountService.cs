using MultiShop.DtoLayer.CatalogDtos.OfferDiscountDtos;

namespace MultiShop.WebUI.Services.CatalogServices.OfferDiscountServices
{
    public class OfferDiscountService : IOfferDiscountService
    {
        private readonly HttpClient _httpClient;

        public OfferDiscountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> CreateOfferDiscountAsync(CreateOfferDiscountDto createOfferDiscountDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("offerdiscounts", createOfferDiscountDto);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<string?> DeleteOfferDiscountAsync(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync("offerdiscounts?id=" + id);
            return await responseMessage.GetErrorMessageAsync();
        }

        public async Task<List<ResultOfferDiscountDto>> GetAllOfferDiscountAsync()
        {
            var responseMessage = await _httpClient.GetAsync("offerdiscounts");
            return await responseMessage.ReadOrThrowAsync<List<ResultOfferDiscountDto>>();
        }

        public async Task<UpdateOfferDiscountDto> GetByIdOfferDiscountAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("offerdiscounts/" + id);
            return await responseMessage.ReadOrThrowAsync<UpdateOfferDiscountDto>();
        }

        public async Task<string?> UpdateOfferDiscountAsync(UpdateOfferDiscountDto updateOfferDiscountDto)
        {
            var responseMessage = await _httpClient.PutAsJsonAsync("offerdiscounts", updateOfferDiscountDto);
            return await responseMessage.GetErrorMessageAsync();
        }
    }
}
