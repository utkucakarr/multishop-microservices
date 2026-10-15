using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.PaymentDto;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.Services.PaymentServices;

namespace MultiShop.WebUI.Controllers
{
    // Ödeme sayfası siparişin durumunu izler: sipariş hazırlanırken (stok ayrılıyor) bekleme ekranı, ödeme beklerken kart
    // formu, iptal edildiyse sebebi. Tutar Payment'ta siparişten alınır; WebUI tutar göndermez, siparişi kendisi açmaz.
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBasketService _basketService;
        private readonly IOrderOrderingService _orderOrderingService;

        public PaymentController(IPaymentService paymentService, IBasketService basketService, IOrderOrderingService orderOrderingService)
        {
            _paymentService = paymentService;
            _basketService = basketService;
            _orderOrderingService = orderOrderingService;
        }

        [HttpGet]
        /// <param name="cancelling">İptal istendi: sipariş Cancelled olana kadar bekleme ekranı gösterilir.</param>
        public async Task<IActionResult> Index(Guid orderNumber, bool cancelling = false)
        {
            if (orderNumber == Guid.Empty)
                return RedirectToAction("Index", "Order");

            var order = await _orderOrderingService.GetByNumberAsync(orderNumber);
            if (OrderStatusDisplay.IsPaid(order?.Status))
            {
                TempData["SuccessMessage"] = "Bu siparişin ödemesi alındı.";
                return RedirectToAction("MyOrderList", "MyOrder", new { area = "User" });
            }

            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Ödeme Ekranı";
            ViewBag.directory3 = "Kartla Ödeme";
            return View(new PaymentPageViewModel
            {
                OrderNumber = orderNumber,
                Order = order,
                Cancelling = cancelling && OrderStatusDisplay.IsOpen(order?.Status ?? OrderStatusDisplay.Pending)
            });
        }

        /// <summary>Bekleme ekranının sorguladığı durum (JSON). Sipariş henüz oluşmadıysa Pending.</summary>
        [HttpGet]
        public async Task<IActionResult> Status(Guid orderNumber)
        {
            var order = await _orderOrderingService.GetByNumberAsync(orderNumber);
            return Json(new { status = order?.Status ?? OrderStatusDisplay.Pending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(CreatePaymentDto createPaymentDto)
        {
            var response = await _paymentService.CreatePaymentAsync(createPaymentDto);
            if (response.IsSuccess)
            {
                // Sepet ancak ödeme alınınca silinir: ödeme başarısız olsa ya da sipariş iptal edilse sepet kaybolmasın.
                await _basketService.DeleteBasketAsync();
                TempData["SuccessMessage"] = "Ödemeniz alındı, siparişiniz onaylandı.";
                return RedirectToAction("MyOrderList", "MyOrder", new { area = "User" });
            }

            TempData["ErrorMessage"] = response.RemainingAttempts > 0
                ? $"{response.ErrorMessage} Kalan deneme hakkınız: {response.RemainingAttempts}."
                : response.ErrorMessage;
            return RedirectToAction(nameof(Index), new { orderNumber = createPaymentDto.OrderNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid orderNumber)
        {
            var error = await _orderOrderingService.CancelAsync(orderNumber);
            if (error is not null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Index), new { orderNumber });
            }
            // İptali saga arka planda işler: sayfa sipariş Cancelled olana kadar bekler.
            return RedirectToAction(nameof(Index), new { orderNumber, cancelling = true });
        }
    }
}
