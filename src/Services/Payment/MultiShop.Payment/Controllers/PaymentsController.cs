using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Payment.Dtos.PaymentDtos;
using MultiShop.Payment.Services.PaymentServices;

namespace MultiShop.Payment.Controllers
{
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUser;

        public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUser)
        {
            _paymentService = paymentService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Ödeme bekleyen siparişi kartla öder. Tutar siparişten gelir. Banka reddederse 200 + <c>isSuccess: false</c>
        /// (kalan hak <c>remainingAttempts</c>); sipariş ödenemez durumdaysa 409, banka yanıt vermezse 503 (deneme sayılmaz).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            var response = await _paymentService.CreatePaymentAsync(_currentUser.GetRequiredUserId(), createPaymentDto);
            return Ok(response);
        }
    }
}
