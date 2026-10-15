using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Authorize]
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
            // Kullanıcı ID'si gönderilmiyor; Order servisi token'dan belirliyor. Liste en yeni sipariş önce gelir.
            var values = await _orderOrderingService.GetMyOrderingsAsync();
            return View(values);
        }

        [Route("User/MyOrder/MyOrderDetail/{orderingId}")]
        public async Task<IActionResult> MyOrderDetail(int orderingId)
        {
            var orderDetails = await _orderDetailService.GetOrderDetailByOrderingId(orderingId);
            if (orderDetails is null)
                return NotFound();
            return View(orderDetails);
        }

        /// <summary>Ödenmemiş siparişi iptal ettirir; ayrılan stok bırakılır. İptal arka planda birkaç saniyede tamamlanır.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid orderNumber)
        {
            var error = await _orderOrderingService.CancelAsync(orderNumber);
            if (error is null)
                TempData["SuccessMessage"] = "İptal isteğiniz alındı; sipariş birkaç saniye içinde iptal edilecek.";
            else
                TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(MyOrderList));
        }
    }
}
