using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class ShoppingCardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBasketService _basketService;
        private readonly IIdentityService _identityService;

        public ShoppingCardController(IProductService productService, IBasketService basketService, IIdentityService identityService)
        {
            _productService = productService;
            _basketService = basketService;
            _identityService = identityService;
        }

        public async Task<IActionResult> Index(string code, int discountRate, decimal totalNewPriceWithDiscount)
        {
            //if (!_identityService.IsAuthenticated())
            //{
            //    return RedirectToAction("Index", "Login");
            //}
            ViewBag.code = code;
            ViewBag.discountRate = discountRate;
            ViewBag.totalNewPriceWithDiscount = totalNewPriceWithDiscount;
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Ürünler";
            ViewBag.directory3 = "Sepetim";
            var values = await _basketService.GetBasket();
            ViewBag.total = values.TotalPrice;
            var tax = values.TotalPrice * 10 / 100;
            ViewBag.tax = tax;
            var totalPriceWithTax = values.TotalPrice + (values.TotalPrice * 10 / 100);
            ViewBag.totalPriceWithTax = totalPriceWithTax;
            return View();
        }

        public async Task<IActionResult> AddBasketItem(string id)
        {
            var values = await _productService.GetByIdProductAsync(id);
            var items = new BasketItemDto
            {
                ProductId = values.ProductId,
                ProductName = values.ProductName,
                Price = values.ProductPrice,
                Quantity = 1,
                ProductImageUrl = values.ProductImageUrl
            };
            await _basketService.AddBasketItem(items);
            return RedirectToAction("Index");
        }


        [Route("shoppingCard/removeAll/{productId}")]
        public async Task<IActionResult> RemoveAllBasketItem(string productId)
        {
            await _basketService.RemoveAllBasketItem(productId);
            return RedirectToAction(nameof(Index));
        }

        [Route("shoppingCard/remove/{productId}")]
        public async Task<IActionResult> RemoveBasketItem(string productId)
        {
            await _basketService.RemoveBasketItem(productId);
            return RedirectToAction(nameof(Index));
        }


        [Route("shoppingCard/add/{productId}")]
        public async Task<IActionResult> AddBasketBtnItem(BasketItemDto basketItemDto)
        {
            await _basketService.AddBasketBtnItem(basketItemDto);
            return RedirectToAction(nameof(Index));
        }


    }
}
