using MultiShop.DtoLayer.PaymentDto;

namespace MultiShop.WebUI.Services.PaymentServices
{
    public interface IPaymentService
    {
        /// <summary>
        /// Ödeme bekleyen siparişi kartla öder; tutar Payment'ta siparişten alınır. Banka reddi, ödenemez sipariş (409)
        /// ve servis hatası (503) <c>IsSuccess = false</c> + mesaj olarak döner.
        /// </summary>
        Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
    }
}
