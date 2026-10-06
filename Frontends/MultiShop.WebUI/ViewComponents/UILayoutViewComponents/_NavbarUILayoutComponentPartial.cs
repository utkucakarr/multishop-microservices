using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;
using MultiShop.WebUI.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace MultiShop.WebUI.ViewComponents.UILayoutViewComponents
{
    public class _NavbarUILayoutComponentPartial : ViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly IIdentityService _identityService;
        private readonly IBasketService _basketService;

        public _NavbarUILayoutComponentPartial(ICategoryService categoryService, IIdentityService identityService, IBasketService basketService)
        {
            _categoryService = categoryService;
            _identityService = identityService;
            _basketService = basketService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isLogin = _identityService.IsAuthenticated();
            var values = await _categoryService.GetAllCategoryAsync();
            ViewBag.basketCount = 0;
            if (isLogin)
            {
                var basket = await _basketService.GetBasket();
                ViewBag.basketCount = basket?.BasketItems.Count ?? 0;
            }
            ViewBag.isLogin = isLogin;

            return View(values);
        }
    }
}