using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.ViewComponents.OrderViewComponents;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Area("User")]
    public class MyOrderController : Controller
    {
        private readonly IOrderOrderingService _orderOrderingService;
        private readonly IOrderDetailService _orderDetailService;

        public MyOrderController(IOrderOrderingService orderOrderingService, IOrderDetailService orderDetailService)
        {
            _orderOrderingService = orderOrderingService;
            _orderDetailService = orderDetailService;
        }

        public async Task<IActionResult> MyOrderList()
        {
            // Kullanıcı ID'si artık gönderilmiyor; Order servisi token'dan belirliyor.
            var values = await _orderOrderingService.GetMyOrderingsAsync();
            var orderedValues = values.OrderByDescending(x => x.OrderDate).ToList();
            return View(orderedValues);
        }

        [Route("User/MyOrder/MyOrderDetail/{orderingId}")]
        public async Task<IActionResult> MyOrderDetail(int orderingId)
        {
            var orderDetails = await _orderDetailService.GetOrderDetailByOrderingId(orderingId);
            if (orderDetails is null)
                return NotFound();
            return View(orderDetails);
        }
    }
}
