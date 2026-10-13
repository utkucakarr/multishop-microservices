using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.NotificationServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Bildirim kutusu (Notification servisi): liste, detay ve gönderim durumları, okundu, başarısız gönderimi tekrar deneme.
    // Token ucu: tarayıcıdaki SignalR istemcisi (wwwroot/js/admin-notifications.js) hub'a bağlanırken kullanır.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Notification")]
    public class NotificationController : Controller
    {
        private const int PageSize = 20;
        private const string ErrorKey = "NotificationError";

        private readonly INotificationService _notificationService;
        private readonly IUserAccessTokenService _userAccessTokenService;

        public NotificationController(INotificationService notificationService, IUserAccessTokenService userAccessTokenService)
        {
            _notificationService = notificationService;
            _userAccessTokenService = userAccessTokenService;
        }

        private void SetViewBag(string title)
        {
            ViewBag.v0 = "Bildirimler";
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Bildirimler";
            ViewBag.v3 = title;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(int page = 1, bool unreadOnly = false)
        {
            SetViewBag("Bildirim Listesi");
            ViewBag.UnreadOnly = unreadOnly;
            var result = await _notificationService.GetListAsync(page, PageSize, unreadOnly);
            if (result is null)
                ViewBag.ServiceError = "Bildirim servisine ulaşılamadı.";
            return View(result);
        }

        [HttpGet("Detail/{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            SetViewBag("Bildirim Detayı");
            var notification = await _notificationService.GetDetailAsync(id);
            if (notification is null)
                return NotFound();

            // Detayı açmak okundu sayılır (okundu bilgisi tüm admin'ler için ortak).
            if (notification.ReadAt is null)
                await _notificationService.MarkAsReadAsync(id);

            return View(notification);
        }

        [HttpPost("MarkAllRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            if (await _notificationService.MarkAllAsReadAsync() is string error)
                TempData[ErrorKey] = error;
            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }

        [HttpPost("Retry/{deliveryId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retry(Guid deliveryId, Guid notificationId)
        {
            if (await _notificationService.RetryDeliveryAsync(deliveryId) is string error)
                TempData[ErrorKey] = error;
            return RedirectToAction(nameof(Detail), new { area = "Admin", id = notificationId });
        }

        // admin-notifications.js: bağlanınca/yeniden bağlanınca zil sayacını durumdan yeniden okur (kaçan push'ların telafisi).
        [HttpGet("UnreadCount")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            return count is null ? StatusCode(StatusCodes.Status503ServiceUnavailable) : Json(count);
        }

        [HttpGet("Token")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Token()
        {
            // Süresi dolmak üzereyse yenilenmiş token döner; yeniden bağlanmada istemci yine buraya gelir.
            var accessToken = await _userAccessTokenService.GetAccessTokenAsync();
            if (accessToken is null)
                return Unauthorized();

            return Json(new { accessToken });
        }
    }
}
