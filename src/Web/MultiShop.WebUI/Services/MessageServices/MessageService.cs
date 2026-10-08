using MultiShop.DtoLayer.MessageDtos;

namespace MultiShop.WebUI.Services.MessageServices
{
    public class MessageService : IMessageService
    {
        private readonly HttpClient _httpClient;

        public MessageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Kullanıcı ID'si gönderilmiyor; Message servisi token'daki kullanıcının kutusunu döner.
        public async Task<List<ResultInboxMessageDto>> GetInboxMessageAsync()
        {
            var responseMessage = await _httpClient.GetAsync("UserMessage/inbox");
            var values = await responseMessage.Content.ReadFromJsonAsync<List<ResultInboxMessageDto>>();
            return values;
        }

        public async Task<List<ResultSendboxMessageDto>> GetSendboxMessageAsync()
        {
            var responseMessage = await _httpClient.GetAsync("UserMessage/sendbox");
            var values = await responseMessage.Content.ReadFromJsonAsync<List<ResultSendboxMessageDto>>();
            return values;
        }

        public async Task<int> GetInboxMessageCountAsync()
        {
            var responseMessage = await _httpClient.GetAsync("UserMessage/inbox/count");
            var values = await responseMessage.Content.ReadFromJsonAsync<int>();
            return values;
        }
    }
}