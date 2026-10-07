using Microsoft.AspNetCore.Mvc;
using MultiShop.Basket.Dtos;
using MultiShop.Basket.Services;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Basket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketService _basketService;
        private readonly ICurrentUserService _currentUser;

        public BasketsController(IBasketService basketService, ICurrentUserService currentUser)
        {
            _basketService = basketService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyBasketDetail()
        {
            var values = await _basketService.GetBasket(_currentUser.GetRequiredUserId());
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMyBasket(BasketTotalDto basketTotalDto)
        {
            basketTotalDto.UserId = _currentUser.GetRequiredUserId();
            await _basketService.SaveBasket(basketTotalDto);
            return Ok("Sepetteki değişiklikler kaydedildi");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMyBasket()
        {
            await _basketService.DeleteBasket(_currentUser.GetRequiredUserId());
            return Ok("Sepet başarıyla silindi");
        }
    }
}
