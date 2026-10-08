using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Authorize]
    [Area("User")]
    public class LogoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
