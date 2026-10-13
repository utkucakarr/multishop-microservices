using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Notification.Dtos;
using MultiShop.Notification.Services;

namespace MultiShop.Notification.Controllers;

[Authorize(Policy = MultiShopPolicies.Admin)]
[Route("api/notification-settings")]
[ApiController]
public class NotificationSettingsController : ControllerBase
{
    private readonly INotificationSettingsService _settingsService;

    public NotificationSettingsController(INotificationSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _settingsService.GetAsync(cancellationToken));

    [HttpPut("rules")]
    public async Task<IActionResult> UpdateRules(List<UpdateNotificationRuleDto> rules, CancellationToken cancellationToken)
    {
        await _settingsService.UpdateRulesAsync(rules, cancellationToken);
        return NoContent();
    }

    [HttpPost("recipients")]
    public async Task<IActionResult> AddRecipient(CreateNotificationRecipientDto recipient, CancellationToken cancellationToken)
        => Ok(await _settingsService.AddRecipientAsync(recipient, cancellationToken));

    [HttpPut("recipients/{id:guid}")]
    public async Task<IActionResult> UpdateRecipient(Guid id, UpdateNotificationRecipientDto recipient, CancellationToken cancellationToken)
    {
        await _settingsService.SetRecipientActiveAsync(id, recipient.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpDelete("recipients/{id:guid}")]
    public async Task<IActionResult> DeleteRecipient(Guid id, CancellationToken cancellationToken)
    {
        await _settingsService.DeleteRecipientAsync(id, cancellationToken);
        return NoContent();
    }
}
