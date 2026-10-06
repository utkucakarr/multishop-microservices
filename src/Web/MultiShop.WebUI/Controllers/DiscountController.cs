using AspNetCoreGeneratedDocument;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.DiscountServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using System.Threading.Tasks;

namespace MultiShop.WebUI.Controllers
{
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IBasketService _basketService;
        private readonly IUserService _userService;
        private readonly IOrderOrderingService _orderOrderingService;
        private readonly IOrderDetailService _orderDetailService;
        public DiscountController(IDiscountService discountService, IBasketService basketService, IUserService userService, IOrderOrderingService orderOrderingService, IOrderDetailService orderDetailService)
        {
            _discountService = discountService;
            _basketService = basketService;
            _userService = userService;
            _orderOrderingService = orderOrderingService;
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public PartialViewResult ConfirmDiscountCoupon()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDiscountCoupon(string code)
        {
            var values = await _discountService.GetDiscountCouponCountRate(code);
            if (values != 0)
            {
                var basketValues = await _basketService.GetBasket();
                var totalPriceWithTax = basketValues.TotalPrice + (basketValues.TotalPrice * 10 / 100);

                var totalNewPriceWithDiscount = totalPriceWithTax - (totalPriceWithTax * values / 100);

                return RedirectToAction("Index", "ShoppingCard", new { code = code, discountRate = values, totalNewPriceWithDiscount = totalNewPriceWithDiscount });
            }

            else
            {
                TempData["ErrorMessage"] = "Hatalı kupon kodu girdiniz";
                return RedirectToAction("Index", "ShoppingCard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOder(CreateOrderingDto createOrderingDto)
        {
            //var user = await _userService.GetUserInfo();
            //createOrderingDto.UserId = user.Id;

            //var basket = await _basketService.GetBasket();
            //createOrderingDto.OrderDate = DateTime.Now;
            //createOrderingDto.TotalPrice = basket.TotalPrice;

            //var orderingId = await _orderOrderingService.CreateOrderingAsync(createOrderingDto);

            //foreach (var item in basket.BasketItems)
            //{
            //    var createOrderDetailDto = new CreateOrderDetailDto
            //    {
            //        OrderingId = orderingId,
            //        ProductId = item.ProductId,
            //        ProductName = item.ProductName,
            //        ProductPrice = item.Price,
            //        ProductAmount = item.Quantity,
            //        ProductTotalPrice = createOrderingDto.TotalPrice
            //    };
            //    await _orderDetailService.CreateOrderDetailAsync(createOrderDetailDto);
            //}

            return RedirectToAction("Index", "Order");
        }
    }
}
