using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.WebUI.Services.BasketServices;

namespace MultiShop.WebUI.Controllers
{
    // Sepet ekranı yalnızca gösterir: fiyat, indirim, KDV, kargo ve genel toplam Basket servisinden gelir (A-02).
    // Sepeti değiştiren işlemler POST + antiforgery (ürün kartlarındaki ve sepetteki butonlar küçük formlardır).
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

        // Giriş yapmamış ziyaretçi "Sepete ekle"ye basınca login'e gider; girişten sonra tarayıcı bu adrese GET ile döner.
        // GET sepeti değiştirmez, yalnızca ürün sayfasına götürür (buton orada bir kez daha basılır).
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddBasketItem(string id)
            => RedirectToAction("ProductDetail", "ProductList", new { id });

        [HttpPost]
        [ActionName(nameof(AddBasketItem))]
        public async Task<IActionResult> AddBasketItemPost(string id)
        {
            ShowError(await _basketService.AddItemAsync(id));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("shoppingCard/removeAll/{productId}")]
        public async Task<IActionResult> RemoveAllBasketItem(string productId)
        {
            ShowError(await _basketService.RemoveItemAsync(productId));
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Adedi bir azaltır; son adet "eksi" ile silinmez (çarpı butonu siler).</summary>
        [HttpPost]
        [Route("shoppingCard/remove/{productId}")]
        public async Task<IActionResult> RemoveBasketItem(string productId, int quantity)
        {
            if (quantity > 1)
                ShowError(await _basketService.SetQuantityAsync(productId, quantity - 1));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
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
