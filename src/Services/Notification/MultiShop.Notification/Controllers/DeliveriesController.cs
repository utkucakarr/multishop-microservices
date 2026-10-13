using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Notification.Services;

namespace MultiShop.Notification.Controllers;

[Authorize(Policy = MultiShopPolicies.Admin)]
[Route("api/deliveries")]
[ApiController]
public class DeliveriesController : ControllerBase
{
    private readonly INotificationInboxService _inboxService;

    public DeliveriesController(INotificationInboxService inboxService)
    {
        _inboxService = inboxService;
    }

    // Kalıcı olarak başarısız (Failed) gönderimi tekrar kuyruğa alır; DeliveryProcessor hemen dener.
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken)
    {
        await _inboxService.RetryDeliveryAsync(id, cancellationToken);
        return NoContent();
    }
}
