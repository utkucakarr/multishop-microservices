using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices;

namespace MultiShop.WebUI.Areas.User.ViewComponents.UserLayoutViewComponents
{
    public class _UserLayoutNavbarComponentPartial : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;

        public _UserLayoutNavbarComponentPartial(IUserService userService, IMessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userService.GetUserInfo();
            ViewBag.userName = user.UserName + " " + user.Surname;
            var inboxMessage = await _messageService.GetInboxMessageAsync(user.Id);
            ViewBag.inboxMessageCount = inboxMessage.Count();
            var senboxMessage = await _messageService.GetSendboxMessageAsync(user.Id);
            ViewBag.sendboxMessageCount = senboxMessage.Count();
            return View();
        }
    }
}
