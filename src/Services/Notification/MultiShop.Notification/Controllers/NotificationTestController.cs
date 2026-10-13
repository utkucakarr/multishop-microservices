using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Notification.Notifications;

namespace MultiShop.Notification.Controllers;

/// <summary>
/// Yalnızca Development: admin paneline test bildirimi gönderir. Event'ler (KT-10) gelene kadar hub'ı
/// uçtan uca denemenin yolu budur. Servisin başka bir REST ucu yoktur.
/// </summary>
[Authorize(Policy = MultiShopPolicies.Admin)]
[Route("api/notifications")]
[ApiController]
public class NotificationTestController : ControllerBase
{
    private readonly IAdminNotifier _adminNotifier;
    private readonly IWebHostEnvironment _environment;

    public NotificationTestController(IAdminNotifier adminNotifier, IWebHostEnvironment environment)
    {
        _adminNotifier = adminNotifier;
        _environment = environment;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        await _adminNotifier.NotifyAdminsAsync("test", "Test bildirimi", "Notification servisi çalışıyor.", "/Admin/Statistic/Index", cancellationToken);
        return Ok("Test bildirimi gönderildi");
    }
}
