using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;
using MultiShop.WebUI.Services.StatisticServices.CommentStatisticServices;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutHeaderComponentPartial : ViewComponent
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly IUserService _userService;
        private readonly ICommentStatisticService _commentStatisticService;

        public _AdminLayoutHeaderComponentPartial(IContactMessageService contactMessageService, IUserService userService, ICommentStatisticService commentStatisticService)
        {
            _contactMessageService = contactMessageService;
            _userService = userService;
            _commentStatisticService = commentStatisticService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userService.GetUserInfo();
            // Zarf simgesi: okunmamış iletişim mesajı sayısı ve en yeni 5 mesaj.
            var contactMessageCount = await _contactMessageService.GetCountAsync();
            ViewBag.messageCount = contactMessageCount.Unread;
            ViewBag.latestContactMessages = await _contactMessageService.GetAllAsync(take: 5);
            int commentCount = await _commentStatisticService.GetTotalCommentCountAsync();
            ViewBag.commentCount = commentCount;
            ViewBag.userName = user.UserName + " " + user.Surname;
            ViewBag.userMail = user.Email;
            return View();
        }
    }
}
