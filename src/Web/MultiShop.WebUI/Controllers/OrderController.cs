using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.OrderServices.OrderAddressServices;

namespace MultiShop.WebUI.Controllers
{
    // Sipariş onayı: teslimat adresi seçilir (ya da yeni adres kaydedilir), sepet Basket'te onaylanır. Siparişi Order'daki
    // saga oluşturur (stok ayırır); kullanıcı ödeme sayfasında siparişin ödeme beklemesini görür.
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderAddressService _orderAddressService;
        private readonly IBasketService _basketService;

        public OrderController(IOrderAddressService orderAddressService, IBasketService basketService)
        {
            _orderAddressService = orderAddressService;
            _basketService = basketService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var basket = await _basketService.GetBasketAsync();
            if (basket is null || basket.Items.Count == 0)
            {
                TempData["ErrorMessage"] = basket is null ? "Sepet şu anda yüklenemiyor." : "Sepetiniz boş.";
                return RedirectToAction("Index", "ShoppingCard");
            }

            var addresses = await _orderAddressService.GetMyAddressesAsync();
            SetBreadcrumb();
            return View(new OrderCheckoutViewModel
            {
                Addresses = addresses,
                SelectedAddressId = addresses.FirstOrDefault()?.AddressId ?? 0,
                ExpectedTotal = OrderCheckoutViewModel.FormatTotal(basket.GrandTotal)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(OrderCheckoutViewModel model)
        {
            var addressId = model.SelectedAddressId;
            if (addressId == 0)
            {
                var (createdId, error) = await _orderAddressService.CreateOrderAddressAsync(model.NewAddress);
                if (createdId is null)
                {
                    ViewBag.errorMessage = error;
                    model.Addresses = await _orderAddressService.GetMyAddressesAsync();
                    SetBreadcrumb();
                    return View(model);
                }
                addressId = createdId.Value;
            }

            var (orderNumber, checkoutError, basketChanged) = await _basketService.CheckoutAsync(addressId, model.ExpectedTotalAmount);
            if (orderNumber is null)
            {
                TempData["ErrorMessage"] = checkoutError;
                // Sepet ya da tutar değişti: kullanıcı güncel sepeti görüp yeniden onaylasın.
                return basketChanged ? RedirectToAction("Index", "ShoppingCard") : RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Index", "Payment", new { orderNumber });
        }

        private void SetBreadcrumb()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Siparişler";
            ViewBag.directory3 = "Sipariş Onayı";
        }
    }
}
