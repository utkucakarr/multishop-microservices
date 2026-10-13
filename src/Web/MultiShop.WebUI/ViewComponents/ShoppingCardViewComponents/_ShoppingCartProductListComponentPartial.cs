using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;

namespace MultiShop.WebUI.ViewComponents.ShoppingCardViewComponents
{
    // Sepeti sayfa (ShoppingCardController.Index) bir kez okur ve buraya verir; bileşen ikinci kez istemez.
    public class _ShoppingCartProductListComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke(BasketDto basket)
        {
            return View(basket);
        }
    }
}
