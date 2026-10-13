using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using MultiShop.DtoLayer.PaymentDto;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.Services.PaymentServices;
using System.Globalization;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBasketService _basketService;
        private readonly IUserService _userService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderOrderingService _orderOrderingService;

        public PaymentController(IPaymentService paymentService, IBasketService basketService, IUserService userService, IOrderDetailService orderDetailService, IOrderOrderingService orderOrderingService)
        {
            _paymentService = paymentService;
            _basketService = basketService;
            _userService = userService;
            _orderDetailService = orderDetailService;
            _orderOrderingService = orderOrderingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string errorMessage)
        {
            var basket = await _basketService.GetBasketAsync();
            if (basket is null || basket.Items.Count == 0)
            {
                TempData["ErrorMessage"] = basket is null ? "Sepet şu anda yüklenemiyor." : "Sepetiniz boş.";
                return RedirectToAction("Index", "ShoppingCard");
            }

            ViewBag.grandTotal = basket.GrandTotal;
            ViewBag.errorMessage = errorMessage;
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Ödeme Ekranı";
            ViewBag.directory3 = "Kartla Ödeme";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Payment(CreatePaymentDto createPaymentDto)
        {
            var user = await _userService.GetUserInfo();
            createPaymentDto.UserID = user.Id;

            // Tutar Basket'in hesapladığı genel toplam (ara toplam − indirim + KDV + kargo); formdan ya da URL'den alınmaz.
            var basket = await _basketService.GetBasketAsync();
            if (basket is null || basket.Items.Count == 0)
            {
                TempData["ErrorMessage"] = basket is null ? "Sepet şu anda yüklenemiyor." : "Sepetiniz boş.";
                return RedirectToAction("Index", "ShoppingCard");
            }
            createPaymentDto.PaymentAmounth = basket.GrandTotal.ToString("0.00", CultureInfo.InvariantCulture);

            var orderRequest = new CreateOrderingDto
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = basket.GrandTotal
            };


            var orderingId = await _orderOrderingService.CreateOrderingAsync(orderRequest);

            foreach (var item in basket.Items)
            {
                var createOrderDetailDto = new CreateOrderDetailDto
                {
                    OrderingId = orderingId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductPrice = item.UnitPrice,
                    ProductAmount = item.Quantity,
                    ProductTotalPrice = item.LineTotal
                };
                await _orderDetailService.CreateOrderDetailAsync(createOrderDetailDto);
            }

            createPaymentDto.OrderingId = orderingId;
            var response = await _paymentService.CreatePaymentAsync(createPaymentDto);

            if (response.IsSuccess == true)
            {
                await _basketService.DeleteBasketAsync();
                //return RedirectToAction("Index", "Default");
                return RedirectToAction("MyOrderList", "MyOrder", new { area = "User" });
            }

            else
            {
                return RedirectToAction("Index", "Payment", new { errorMessage = response.ErrorMessage });
            }

        }

    }
}