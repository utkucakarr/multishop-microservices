using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderDetailServices
{
    public interface IOrderDetailService
    {
        Task CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetailDto);

        Task<List<GetOrderDetailByOrderIdDto>> GetOrderDetailByOrderingId(int orderingId);
    }
}
