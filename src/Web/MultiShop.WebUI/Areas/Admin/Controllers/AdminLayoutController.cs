using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
