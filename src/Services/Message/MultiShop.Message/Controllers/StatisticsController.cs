using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Message.Services;

namespace MultiShop.Message.Controllers
{
    // Admin dashboard istatistikleri: tek uç, Ocelot /services/admin/dashboard bunu diğer servislerle birleştirir.
    // Admin üst menüsündeki zarf da okunmamış sayıyı buradan alır.
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;

        public StatisticsController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var value = await _contactMessageService.GetStatisticsAsync();
            return Ok(value);
        }
    }
}
