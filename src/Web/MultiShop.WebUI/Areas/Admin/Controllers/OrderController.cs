using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Tüm siparişler: durum filtresi, satırlar ve ödenmemiş siparişi iptal. Durumu sipariş saga'sı yönetir; admin
    // durumu elle değiştiremez, yalnızca iptal isteyebilir (stok bırakılır, bekleyen ödeme kapanır).
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Order")]
    public class OrderController : Controller
    {
        private readonly IOrderOrderingService _orderOrderingService;
        private readonly IOrderDetailService _orderDetailService;

        public OrderController(IOrderOrderingService orderOrderingService, IOrderDetailService orderDetailService)
        {
            _orderOrderingService = orderOrderingService;
            _orderDetailService = orderDetailService;
        }

        void OrderViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Siparişler";
            ViewBag.v3 = "Sipariş Listesi";
            ViewBag.v0 = "Sipariş İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index(string? status)
        {
            OrderViewBagList();
            ViewBag.status = OrderStatusDisplay.All.Contains(status) ? status : null;
            var values = await _orderOrderingService.GetAllAsync(ViewBag.status);
            return View(values);
        }

        [Route("Detail/{orderingId:int}")]
        public async Task<IActionResult> Detail(int orderingId)
        {
            OrderViewBagList();
            var values = await _orderDetailService.GetOrderDetailByOrderingId(orderingId);
            if (values is null)
                return NotFound();
            ViewBag.orderingId = orderingId;
            return View(values);
        }

        [Route("Cancel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid orderNumber, string? status)
        {
            var error = await _orderOrderingService.CancelAsync(orderNumber);
            if (error is null)
                TempData["SuccessMessage"] = "İptal isteği gönderildi; sipariş birkaç saniye içinde iptal edilecek.";
            else
                TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(Index), new { status });
        }
    }
}
