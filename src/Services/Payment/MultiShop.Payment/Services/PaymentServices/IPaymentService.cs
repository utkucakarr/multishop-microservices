using MultiShop.Payment.Dtos.PaymentDtos;

namespace MultiShop.Payment.Services.PaymentServices
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
    }
}
