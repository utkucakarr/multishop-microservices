using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;
using MultiShop.WebUI.Services.StatisticServices;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutHeaderComponentPartial : ViewComponent
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly IUserService _userService;
        private readonly IDashboardService _dashboardService;

        public _AdminLayoutHeaderComponentPartial(IContactMessageService contactMessageService, IUserService userService, IDashboardService dashboardService)
        {
            _contactMessageService = contactMessageService;
            _userService = userService;
            _dashboardService = dashboardService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userService.GetUserInfo();
            // Zarf simgesi: okunmamış iletişim mesajı sayısı ve en yeni 5 mesaj.
            // Servis yanıt vermezse rozet boş kalır (sayfa çökmez).
            var messageStatistics = await _dashboardService.GetMessageStatisticsAsync();
            ViewBag.messageCount = messageStatistics?.Unread;
            ViewBag.latestContactMessages = await _contactMessageService.GetAllAsync(take: 5);
            var commentStatistics = await _dashboardService.GetCommentStatisticsAsync();
            ViewBag.commentCount = commentStatistics?.Total;
            ViewBag.userName = user.UserName + " " + user.Surname;
            ViewBag.userMail = user.Email;
            return View();
        }
    }
}
