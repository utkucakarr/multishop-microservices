using Microsoft.AspNetCore.Mvc;
using MultiShop.Basket.Dtos;
using MultiShop.Basket.Services;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Basket.Controllers
{
    // Sepet her zaman token'daki kullanıcınındır. İstemci yalnızca ürün id'si, adet ve kupon kodu gönderir;
    // fiyat ve toplamlar sunucuda hesaplanır (BAS-01, BAS-06, A-02).
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

        private string UserId => _currentUser.GetRequiredUserId();

        /// <summary>Güncel fiyatlarla sepet ve toplamlar (ara toplam, indirim, KDV, kargo, genel toplam).</summary>
        [HttpGet]
        public async Task<ActionResult<BasketDto>> GetMyBasket()
            => Ok(await _basketService.GetBasketAsync(UserId));

        /// <summary>Sepetteki farklı ürün sayısı (üst menü rozeti); fiyatlama yapmaz.</summary>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetMyBasketItemCount()
            => Ok(await _basketService.GetItemCountAsync(UserId));

        /// <summary>Ürün ekler; sepette varsa adedi artırır.</summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItem(AddBasketItemDto item)
        {
            await _basketService.AddItemAsync(UserId, item);
            return NoContent();
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> SetQuantity(string productId, UpdateBasketItemDto item)
        {
            await _basketService.SetQuantityAsync(UserId, productId, item.Quantity);
            return NoContent();
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(string productId)
        {
            await _basketService.RemoveItemAsync(UserId, productId);
            return NoContent();
        }

        /// <summary>Kuponu Discount'ta doğrulayıp sepete uygular; geçersizse 400.</summary>
        [HttpPut("coupon")]
        public async Task<IActionResult> ApplyCoupon(ApplyCouponDto coupon)
        {
            await _basketService.ApplyCouponAsync(UserId, coupon.Code);
            return NoContent();
        }

        [HttpDelete("coupon")]
        public async Task<IActionResult> RemoveCoupon()
        {
            await _basketService.RemoveCouponAsync(UserId);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMyBasket()
        {
            await _basketService.DeleteBasketAsync(UserId);
            return NoContent();
        }
    }
}
