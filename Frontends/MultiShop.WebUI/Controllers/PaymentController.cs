using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using MultiShop.DtoLayer.PaymentDto;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.Services.PaymentServices;

namespace MultiShop.WebUI.Controllers
{
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
        public IActionResult Index(string errorMessage)
        {
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

            var basket = await _basketService.GetBasket();
            createPaymentDto.PaymentAmounth = basket.TotalPrice.ToString("0.##");


            var orderRequest = new CreateOrderingDto
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = basket.TotalPrice
            };


            var orderingId = await _orderOrderingService.CreateOrderingAsync(orderRequest);

            foreach (var item in basket.BasketItems)
            {
                var createOrderDetailDto = new CreateOrderDetailDto
                {
                    OrderingId = orderingId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductPrice = item.Price,
                    ProductAmount = item.Quantity,
                    ProductTotalPrice = orderRequest.TotalPrice
                };
                await _orderDetailService.CreateOrderDetailAsync(createOrderDetailDto);
            }

            createPaymentDto.OrderingId = orderingId;
            var response = await _paymentService.CreatePaymentAsync(createPaymentDto);

            if (response.IsSuccess == true)
            {
                await _basketService.DeleteBasket(user.Id);
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