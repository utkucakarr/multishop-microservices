using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Discount.Services;

namespace MultiShop.Discount.Controllers
{
    // Admin dashboard istatistikleri: tek uç, Ocelot /services/admin/dashboard bunu diğer servislerle birleştirir.
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public StatisticsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var value = await _discountService.GetStatisticsAsync();
            return Ok(value);
        }
    }
}
