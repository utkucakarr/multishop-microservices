using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderDetailServices
{
    public interface IOrderDetailService
    {
        /// <summary>Siparişin satırları; sipariş yoksa ya da kullanıcıya ait değilse <c>null</c>.</summary>
        Task<List<GetOrderDetailByOrderIdDto>?> GetOrderDetailByOrderingId(int orderingId);
    }
}
