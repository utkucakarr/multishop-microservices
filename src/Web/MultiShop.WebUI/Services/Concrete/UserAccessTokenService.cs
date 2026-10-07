using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Services.Concrete
{
    public class UserAccessTokenService : IUserAccessTokenService
    {
        // Aynı sayfa isteği içinde birden çok servis çağrısı yapılıyor; token (ve yenileme sonucu) istek boyunca bir kez hesaplanır.
        // Yenilenen token yeni cookie'ye yazılır ama bu istekte okunan cookie hâlâ eskisidir; bu yüzden HttpContext.Items'ta tutulur.
        private const string CacheKey = "MultiShop.UserAccessToken";

        // Süresi dolmak üzere olan token'ı servise göndermeden önce yenile.
        private static readonly TimeSpan RefreshBeforeExpiry = TimeSpan.FromMinutes(1);

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;

        public UserAccessTokenService(IHttpContextAccessor httpContextAccessor, IIdentityService identityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
        }

        public async Task<string?> GetAccessTokenAsync(bool forceRefresh = false)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.User.Identity?.IsAuthenticated != true)
                return null;

            if (!forceRefresh && context.Items.TryGetValue(CacheKey, out var cached))
                return cached as string; // null: bu istekte yenileme zaten başarısız oldu

            var accessToken = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectParameterNames.AccessToken);
            var expiresAt = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "expires_at");

            var isExpiring = !DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiry)
                || expiry - RefreshBeforeExpiry <= DateTimeOffset.UtcNow;

            if (forceRefresh || isExpiring || string.IsNullOrEmpty(accessToken))
                accessToken = await _identityService.RefreshAccessTokenAsync();

            context.Items[CacheKey] = accessToken;
            return accessToken;
        }
    }
}
