using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Notification.Services;

namespace MultiShop.Notification.Controllers;

[Authorize(Policy = MultiShopPolicies.Admin)]
[Route("api/notifications")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly INotificationInboxService _inboxService;

    public NotificationsController(INotificationInboxService inboxService)
    {
        _inboxService = inboxService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? type = null, [FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
        => Ok(await _inboxService.GetListAsync(page, pageSize, type, unreadOnly, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
        => Ok(await _inboxService.GetDetailAsync(id, cancellationToken));

    // Zil sayacı; istemci bağlanınca/yeniden bağlanınca da çağırır (kaçan push'ların telafisi).
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        => Ok(await _inboxService.GetUnreadCountAsync(cancellationToken));

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _inboxService.MarkAsReadAsync(id, CurrentUserName, cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        => Ok(await _inboxService.MarkAllAsReadAsync(CurrentUserName, cancellationToken));

    private string CurrentUserName => User.FindFirst("name")?.Value ?? User.FindFirst("sub")?.Value ?? "admin";
}
