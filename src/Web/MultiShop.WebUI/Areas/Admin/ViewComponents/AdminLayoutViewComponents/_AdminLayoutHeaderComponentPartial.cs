using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices;
using MultiShop.WebUI.Services.StatisticServices.CommentStatisticServices;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutHeaderComponentPartial : ViewComponent
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly ICommentStatisticService _commentStatisticService;

        public _AdminLayoutHeaderComponentPartial(IMessageService messageService, IUserService userService, ICommentStatisticService commentStatisticService)
        {
            _messageService = messageService;
            _userService = userService;
            _commentStatisticService = commentStatisticService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userService.GetUserInfo();
            int messageCount = await _messageService.GetTotalMessageCountByRecieverId(user.Id);
            ViewBag.messageCount = messageCount;
            int commentCount = await _commentStatisticService.GetTotalCommentCountAsync();
            ViewBag.commentCount = commentCount;
            ViewBag.userName = user.UserName + " " + user.Surname;
            ViewBag.userMail = user.Email;
            return View();
        }
    }
}
