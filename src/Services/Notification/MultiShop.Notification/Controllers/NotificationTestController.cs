using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Dispatching;

namespace MultiShop.Notification.Controllers;

/// <summary>
/// Admin panelindeki "Test gönder": bildirim kanallarını uçtan uca dener. <c>channel</c> verilirse kuraldan
/// bağımsız olarak yalnızca o kanal kullanılır; verilmezse "test" türünün kuralı geçerlidir.
/// </summary>
[Authorize(Policy = MultiShopPolicies.Admin)]
[Route("api/notifications")]
[ApiController]
public class NotificationTestController : ControllerBase
{
    private readonly INotificationDispatcher _dispatcher;

    public NotificationTestController(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromQuery] NotificationChannel? channel, CancellationToken cancellationToken)
    {
        var sentBy = User.FindFirst("name")?.Value ?? "admin";
        var request = new NotificationRequest(
            NotificationTypes.Test,
            NotificationTarget.Admins,
            new Dictionary<string, string> { ["SentBy"] = sentBy },
            channel is { } only ? new[] { only } : null);

        var notificationId = await _dispatcher.DispatchAsync(request, cancellationToken);
        return notificationId is null
            ? Ok("Test bildirimi için açık kanal yok.")
            : Ok("Test bildirimi gönderildi");
    }
}
