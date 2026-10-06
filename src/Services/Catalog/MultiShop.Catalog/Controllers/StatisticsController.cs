using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Services.StatisticServices;
using System.Threading.Tasks;

namespace MultiShop.Catalog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("GetBrandCount")]
        public async Task<IActionResult> GetBrandCount()
        {
            var value = await _statisticService.GetBrandCountAsync();
            return Ok(value);
        }

        [HttpGet("GetCategoryCount")]
        public async Task<IActionResult> GetCategoryCount()
        {
            var value = await _statisticService.GetCategoryCountAsync();
            return Ok(value);
        }

        [HttpGet("GetProductCount")]
        public async Task<IActionResult> GetProductCount()
        {
            var value = await _statisticService.GetProductCountAsync();
            return Ok(value);
        }

        [HttpGet("GetProductAvgPrice")]
        public async Task<IActionResult> GetProductAvgPrice()
        {
            var value = _statisticService.GetProductAvgPriceAsync();
            return Ok(value);
        }

        [HttpGet("GetMaxPriceProductName")]
        public async Task<IActionResult> GetMaxPriceProductName()
        {
            var value = await _statisticService.GetMaxPriceProductNameAsync();
            return Ok(value);
        }

        [HttpGet("GetMinPriceProductName")]
        public async Task<IActionResult> GetMinPriceProductName()
        {
            var value = await _statisticService.GetMinPriceProductNameAsync();
            return Ok(value);
        }
    }
}
