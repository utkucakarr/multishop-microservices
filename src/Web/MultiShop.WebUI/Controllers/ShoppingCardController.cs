using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.WebUI.Services.BasketServices;

namespace MultiShop.WebUI.Controllers
{
    // Sepet ekranı yalnızca gösterir: fiyat, indirim, KDV, kargo ve genel toplam Basket servisinden gelir (A-02).
    // Ürün kartlarındaki linkler (/ShoppingCard/AddBasketItem/{id}, /shoppingCard/add|remove|removeAll/{id}) aynı kaldı.
    [Authorize]
    public class ShoppingCardController : Controller
    {
        private readonly IBasketService _basketService;

        public ShoppingCardController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Ürünler";
            ViewBag.directory3 = "Sepetim";

            var basket = await _basketService.GetBasketAsync();
            if (basket is null)
            {
                TempData["ErrorMessage"] ??= "Sepet şu anda yüklenemiyor, lütfen biraz sonra tekrar deneyin.";
                basket = new BasketDto();
            }
            return View(basket);
        }

        public async Task<IActionResult> AddBasketItem(string id)
        {
            ShowError(await _basketService.AddItemAsync(id));
            return RedirectToAction(nameof(Index));
        }

        [Route("shoppingCard/removeAll/{productId}")]
        public async Task<IActionResult> RemoveAllBasketItem(string productId)
        {
            ShowError(await _basketService.RemoveItemAsync(productId));
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Adedi bir azaltır; son adet "eksi" ile silinmez (çarpı butonu siler).</summary>
        [Route("shoppingCard/remove/{productId}")]
        public async Task<IActionResult> RemoveBasketItem(string productId, int quantity)
        {
            if (quantity > 1)
                ShowError(await _basketService.SetQuantityAsync(productId, quantity - 1));
            return RedirectToAction(nameof(Index));
        }

        [Route("shoppingCard/add/{productId}")]
        public async Task<IActionResult> AddBasketBtnItem(string productId)
        {
            ShowError(await _basketService.AddItemAsync(productId));
            return RedirectToAction(nameof(Index));
        }

        private void ShowError(string? error)
        {
            if (error is not null)
                TempData["ErrorMessage"] = error;
        }
    }
}
