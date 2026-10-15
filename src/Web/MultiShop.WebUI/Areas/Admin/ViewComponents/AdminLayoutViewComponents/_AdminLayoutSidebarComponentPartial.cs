using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutSidebarComponentPartial : ViewComponent
    {
        private readonly ICurrentUser _currentUser;

        public _AdminLayoutSidebarComponentPartial(ICurrentUser currentUser)
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
