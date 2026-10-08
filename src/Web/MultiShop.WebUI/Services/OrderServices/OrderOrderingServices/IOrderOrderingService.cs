using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderOrderingServices
{
    public interface IOrderOrderingService
    {
        /// <summary>Giriş yapmış kullanıcının kendi siparişleri; kullanıcı Order servisinde token'dan belirlenir.</summary>
        Task<List<ResultOrderingByUserIdDto>> GetMyOrderingsAsync();

        Task<int> CreateOrderingAsync(CreateOrderingDto createOrderingDto);
    }
}
