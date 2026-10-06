using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;
using MultiShop.DtoLayer.PaymentDto;

namespace MultiShop.WebUI.Services.PaymentServices
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
    }
}
