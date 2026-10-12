using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.StatisticServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    public class StatisticController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public StatisticController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // Önceden 5 servise sırayla 11 istek atılıyordu; şimdi Ocelot beşini paralel çağırıp tek yanıtta birleştiriyor.
        public async Task<IActionResult> Index()
        {
            var dashboard = await _dashboardService.GetDashboardAsync();
            return View(dashboard);
        }
    }
}
