using MultiShop.DtoLayer.PaymentDto;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace MultiShop.WebUI.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        // Gateway üzerinden ve giriş yapmış kullanıcının token'ıyla gider (bkz. Program.cs);
        // önceden doğrudan http://localhost:7076'ya token'sız gidiyordu.
        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var jsonData = JsonConvert.SerializeObject(createPaymentDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await _httpClient.PostAsync("payments", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var responseDto = JsonConvert.DeserializeObject<CreatePaymentResponseDto>(responseContent);
                if (responseDto is not null)
                    return responseDto;
            }

            // Önceden null dönüyordu ve PaymentController response.IsSuccess'te çöküyordu.
            return new CreatePaymentResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Ödeme şu anda alınamıyor, lütfen tekrar deneyin."
            };
        }
    }
}
