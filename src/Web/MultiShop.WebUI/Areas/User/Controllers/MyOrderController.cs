using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.ViewComponents.OrderViewComponents;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Area("User")]
    public class MyOrderController : Controller
    {
        private readonly IOrderOrderingService _orderOrderingService;
        private readonly IUserService _userService;
        private readonly IOrderDetailService _orderDetailService;

        public MyOrderController(IOrderOrderingService orderOrderingService, IUserService userService, IOrderDetailService orderDetailService)
        {
            _orderOrderingService = orderOrderingService;
            _userService = userService;
            _orderDetailService = orderDetailService;
        }

        public async Task<IActionResult> MyOrderList()
        {
            var user = await _userService.GetUserInfo();
            var values = await _orderOrderingService.GetOrderingByUserId(user.Id);
            var orderedValues = values.OrderByDescending(x => x.OrderDate).ToList();
            return View(orderedValues);
        }

        [Route("User/MyOrder/MyOrderDetail/{orderingId}")]
        public async Task<IActionResult> MyOrderDetail(int orderingId)
        {
            var user = await _userService.GetUserInfo();
            var orderDetails = await _orderDetailService.GetOrderDetailByOrderingId(orderingId);
            return View(orderDetails);
        }
    }
}
