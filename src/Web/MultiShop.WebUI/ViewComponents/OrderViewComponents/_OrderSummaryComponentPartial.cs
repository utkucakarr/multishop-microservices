using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.WebUI.Services.BasketServices;

namespace MultiShop.WebUI.ViewComponents.OrderViewComponents
{
    // Sipariş sayfasındaki özet: tutarlar Basket'ten (sepet sayfasıyla ve tahsil edilen tutarla aynı formül).
    public class _OrderSummaryComponentPartial : ViewComponent
    {
        private readonly IBasketService _basketService;

        public _OrderSummaryComponentPartial(IBasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basket = await _basketService.GetBasketAsync() ?? new BasketDto();
            return View(basket);
        }
    }
}
