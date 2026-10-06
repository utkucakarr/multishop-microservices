using Microsoft.EntityFrameworkCore.ChangeTracking;
using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.PaymentDto;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace MultiShop.WebUI.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createPaymentDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("http://localhost:7076/api/Payments", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var responseDto = JsonConvert.DeserializeObject<CreatePaymentResponseDto>(responseContent);
                return responseDto;
            }

            return null;
        }
    }
}
