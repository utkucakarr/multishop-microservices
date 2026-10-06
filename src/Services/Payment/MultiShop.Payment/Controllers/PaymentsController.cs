using Microsoft.AspNetCore.Mvc;
using MultiShop.Payment.Dtos.PaymentDtos;
using MultiShop.Payment.Services.PaymentServices;

namespace MultiShop.Payments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreatePaymentDto createPaymentDto)
        {
            var response = await _paymentService.CreatePaymentAsync(createPaymentDto);
            return Ok(response);
        }
    }
}
