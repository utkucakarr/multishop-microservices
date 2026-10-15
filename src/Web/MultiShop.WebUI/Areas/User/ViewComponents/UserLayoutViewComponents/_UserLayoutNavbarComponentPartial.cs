using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Areas.User.ViewComponents.UserLayoutViewComponents
{
    public class _UserLayoutNavbarComponentPartial : ViewComponent
    {
        private readonly ICurrentUser _currentUser;

        public _UserLayoutNavbarComponentPartial(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.userName = _currentUser.DisplayName;
            return View();
        }
    }
}
