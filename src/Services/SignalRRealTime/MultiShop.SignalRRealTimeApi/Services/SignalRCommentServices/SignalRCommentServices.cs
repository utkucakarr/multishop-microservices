
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MultiShop.SignalRRealTimeApi.Services.SignalRCommentServices
{
    public class SignalRCommentServices : ISignalRCommentServices
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SignalRCommentServices(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<int> GetTotalCommentCountAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("http://localhost:7075/api/CommentStatistics");
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var commentCount = JsonConvert.DeserializeObject<int>(jsonData);
            return commentCount;
        }
    }
}
