using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Payment.Dtos.PaymentDtos;
using MultiShop.Payment.Services.PaymentServices;

namespace MultiShop.Payments.Controllers
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

        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            // İstekteki UserId yok sayılır: ödeme her zaman token'daki kullanıcı adına kaydedilir.
            createPaymentDto.UserId = _currentUser.GetRequiredUserId();
            var response = await _paymentService.CreatePaymentAsync(createPaymentDto);
            return Ok(response);
        }
    }
}
