using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.NotificationDtos;
using MultiShop.WebUI.Services.NotificationServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Bildirim Ayarları: tür × kanal kuralları, admin bildirimlerinin alıcıları (mail/telefon), kanal başına test.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/NotificationSettings")]
    public class NotificationSettingsController : Controller
    {
        private const string ErrorKey = "NotificationSettingsError";
        private const string SuccessKey = "NotificationSettingsSuccess";

        private readonly INotificationService _notificationService;

        public NotificationSettingsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewBag.v0 = "Bildirimler";
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Bildirimler";
            ViewBag.v3 = "Bildirim Ayarları";

            var settings = await _notificationService.GetSettingsAsync();
            if (settings is null)
                ViewBag.ServiceError = "Bildirim servisine ulaşılamadı.";
            return View(settings);
        }

        [HttpPost("UpdateRules")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(List<NotificationRuleDto> rules)
            => Result(await _notificationService.UpdateRulesAsync(rules), "Kurallar kaydedildi.");

        [HttpPost("AddRecipient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecipient(CreateNotificationRecipientDto recipient)
            => Result(await _notificationService.AddRecipientAsync(recipient), "Alıcı eklendi.");

        [HttpPost("SetRecipientActive/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRecipientActive(Guid id, bool isActive)
            => Result(await _notificationService.SetRecipientActiveAsync(id, isActive), isActive ? "Alıcı etkinleştirildi." : "Alıcı devre dışı bırakıldı.");

        [HttpPost("DeleteRecipient/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecipient(Guid id)
            => Result(await _notificationService.DeleteRecipientAsync(id), "Alıcı silindi.");

        [HttpPost("SendTest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTest(string? channel)
            => Result(await _notificationService.SendTestAsync(channel),
                "Test bildirimi gönderildi. Mail/SMS durumunu Bildirimler sayfasındaki detaydan izleyebilirsiniz.");

        private IActionResult Result(string? error, string success)
        {
            if (error is null)
                TempData[SuccessKey] = success;
            else
                TempData[ErrorKey] = error;
            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }
    }
}
