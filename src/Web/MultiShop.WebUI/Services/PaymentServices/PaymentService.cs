using MultiShop.DtoLayer.PaymentDto;

namespace MultiShop.WebUI.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        // Gateway üzerinden ve giriş yapmış kullanıcının token'ıyla gider (bkz. Program.cs).
        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var response = await _httpClient.PostAsJsonAsync("payments", createPaymentDto);
            var error = await response.GetErrorMessageAsync();
            if (error is null)
            {
                var result = await response.Content.ReadFromJsonAsync<CreatePaymentResponseDto>();
                if (result is not null)
                    return result;
                error = "Ödeme sonucu okunamadı, lütfen siparişlerinizi kontrol edin.";
            }

            return new CreatePaymentResponseDto { IsSuccess = false, ErrorMessage = error };
        }
    }
}
