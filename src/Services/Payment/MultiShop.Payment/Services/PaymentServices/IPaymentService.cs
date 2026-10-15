using MultiShop.Payment.Dtos.PaymentDtos;

namespace MultiShop.Payment.Services.PaymentServices
{
    public interface IPaymentService
    {
        /// <summary>Kullanıcının ödeme bekleyen siparişinden kartla tahsilat.</summary>
        Task<CreatePaymentResponseDto> CreatePaymentAsync(string userId, CreatePaymentDto createPaymentDto);
    }
}
