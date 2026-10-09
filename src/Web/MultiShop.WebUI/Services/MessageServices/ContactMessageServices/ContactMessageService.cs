using System.Net;
using MultiShop.DtoLayer.MessageDtos.ContactMessageDtos;

namespace MultiShop.WebUI.Services.MessageServices.ContactMessageServices
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly HttpClient _httpClient;

        public ContactMessageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CreateAsync(CreateContactMessageDto createContactMessageDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("contacts", createContactMessageDto);
            return responseMessage.IsSuccessStatusCode;
        }

        public async Task<List<ResultContactMessageDto>> GetAllAsync(int? take = null)
        {
            var url = take is > 0 ? $"contacts?take={take}" : "contacts";
            return await _httpClient.GetFromJsonAsync<List<ResultContactMessageDto>>(url) ?? new();
        }

        public async Task<ResultContactMessageDto?> GetByIdAsync(int id)
        {
            var responseMessage = await _httpClient.GetAsync($"contacts/{id}");
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                return null;
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadFromJsonAsync<ResultContactMessageDto>();
        }

        public async Task MarkAsReadAsync(int id)
            => (await _httpClient.PutAsync($"contacts/{id}/read", null)).EnsureSuccessStatusCode();

        public async Task DeleteAsync(int id)
            => (await _httpClient.DeleteAsync($"contacts/{id}")).EnsureSuccessStatusCode();

        public async Task<ContactMessageCountDto> GetCountAsync()
            => await _httpClient.GetFromJsonAsync<ContactMessageCountDto>("contacts/count") ?? new();
    }
}
