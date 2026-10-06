using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public async Task<List<ResultInboxMessageDto>> GetInboxMessageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("http://localhost:5020/services/Message/UserMessage/GetMesssageInBox?id=" + id);
            var values = await responseMessage.Content.ReadFromJsonAsync<List<ResultInboxMessageDto>>();
            return values;
        }

        public async Task<List<ResultSendboxMessageDto>> GetSendboxMessageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("http://localhost:5020/services/Message/UserMessage/GetMessageSendBox?id=" + id);
            var values = await responseMessage.Content.ReadFromJsonAsync<List<ResultSendboxMessageDto>>();
            return values;
        }

        public async Task<int> GetTotalMessageCountByRecieverId(string id)
        {
            var responseMessage = await _httpClient.GetAsync("UserMessage/GetTotalMessageCountByRecieverId?id=" + id);
            var values = await responseMessage.Content.ReadFromJsonAsync<int>();
            return values;
        }
    }
}