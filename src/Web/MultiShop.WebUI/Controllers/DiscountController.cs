using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.BasketServices;

namespace MultiShop.WebUI.Controllers
{
    // Kupon sepete Basket servisi üzerinden uygulanır: kodu Discount'ta Basket doğrular, oran ve indirimli tutar
    // artık URL'de taşınmaz (A-02).
    [Authorize]
    public class DiscountController : Controller
    {
        private readonly IBasketService _basketService;

        public DiscountController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDiscountCoupon(string code)
        {
            var error = string.IsNullOrWhiteSpace(code)
                ? "Kupon kodu girin."
                : await _basketService.ApplyCouponAsync(code);
            if (error is not null)
                TempData["ErrorMessage"] = error;
            return RedirectToAction("Index", "ShoppingCard");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDiscountCoupon()
        {
            var error = await _basketService.RemoveCouponAsync();
            if (error is not null)
                TempData["ErrorMessage"] = error;
            return RedirectToAction("Index", "ShoppingCard");
        }

        [HttpPost]
        public IActionResult ConfirmOder()
        {
            return RedirectToAction("Index", "Order");
        }
    }
}
