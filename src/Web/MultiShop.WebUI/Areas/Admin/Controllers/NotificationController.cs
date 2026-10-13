using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Tarayıcıdaki SignalR istemcisi (wwwroot/js/admin-notifications.js) Notification hub'ına bağlanırken
    // token'ı buradan alır: WebSocket'e Authorization başlığı eklenemediği için token ?access_token= ile gider.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Notification")]
    public class NotificationController : Controller
    {
        private readonly IUserAccessTokenService _userAccessTokenService;

        public NotificationController(IUserAccessTokenService userAccessTokenService)
        {
            _userAccessTokenService = userAccessTokenService;
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
