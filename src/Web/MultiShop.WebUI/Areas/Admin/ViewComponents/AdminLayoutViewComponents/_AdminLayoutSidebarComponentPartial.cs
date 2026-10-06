using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutSidebarComponentPartial : ViewComponent
    {
        private readonly IUserService _userService;

        public _AdminLayoutSidebarComponentPartial(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _userService.GetUserInfo();
            ViewBag.userName = values.UserName + " " + values.Surname;
            return View();
        }
    }
}
