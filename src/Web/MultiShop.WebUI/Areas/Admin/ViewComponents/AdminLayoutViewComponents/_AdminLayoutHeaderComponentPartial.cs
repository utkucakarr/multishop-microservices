using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;
using MultiShop.WebUI.Services.NotificationServices;
using MultiShop.WebUI.Services.StatisticServices;

namespace MultiShop.WebUI.Areas.Admin.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutHeaderComponentPartial : ViewComponent
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly ICurrentUser _currentUser;
        private readonly IDashboardService _dashboardService;
        private readonly INotificationService _notificationService;

        public _AdminLayoutHeaderComponentPartial(IContactMessageService contactMessageService, ICurrentUser currentUser, IDashboardService dashboardService, INotificationService notificationService)
        {
            _notificationService = notificationService;
            _contactMessageService = contactMessageService;
            _currentUser = currentUser;
            _dashboardService = dashboardService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Zarf simgesi: okunmamış iletişim mesajı sayısı ve en yeni 5 mesaj.
            // Servis yanıt vermezse rozet boş kalır (sayfa çökmez).
            var messageStatistics = await _dashboardService.GetMessageStatisticsAsync();
            ViewBag.messageCount = messageStatistics?.Unread;
            ViewBag.latestContactMessages = await _contactMessageService.GetAllAsync(take: 5);
            // Zil: okunmamış bildirim sayısı ve en yeni 5 bildirim; admin-notifications.js canlı günceller.
            ViewBag.notificationCount = await _notificationService.GetUnreadCountAsync();
            ViewBag.latestNotifications = (await _notificationService.GetListAsync(1, 5, false))?.Items ?? new List<MultiShop.DtoLayer.NotificationDtos.NotificationListItemDto>();
            // Ad ve e-posta cookie claim'lerinden (Identity'ye istek atılmaz).
            ViewBag.userName = _currentUser.DisplayName;
            ViewBag.userMail = _currentUser.Email;
            return View();
        }
    }
}
