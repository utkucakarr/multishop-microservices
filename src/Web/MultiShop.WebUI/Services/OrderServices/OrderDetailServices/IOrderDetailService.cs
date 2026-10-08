using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderDetailServices
{
    public interface IOrderDetailService
    {
        Task CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetailDto);

        /// <summary>Siparişin satırları; sipariş yoksa ya da kullanıcıya ait değilse <c>null</c>.</summary>
        Task<List<GetOrderDetailByOrderIdDto>?> GetOrderDetailByOrderingId(int orderingId);
    }
}
