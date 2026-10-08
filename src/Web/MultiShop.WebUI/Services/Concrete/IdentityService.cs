using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Settings;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace MultiShop.WebUI.Services.Concrete
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClientSettings _clientSettings;
        private readonly ServiceApiSettings _serviceApiSettings;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<ClientSettings> clientSettings, IOptions<ServiceApiSettings> serviceApiSettings, ILogger<IdentityService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _clientSettings = clientSettings.Value;
            _serviceApiSettings = serviceApiSettings.Value;
            _logger = logger;
        }

        public async Task<string?> RefreshAccessTokenAsync()
        {
            var context = _httpContextAccessor.HttpContext!;
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var refreshToken = authenticateResult.Properties?.GetTokenValue(OpenIdConnectParameterNames.RefreshToken);

            if (!authenticateResult.Succeeded || string.IsNullOrEmpty(refreshToken))
            {
                // KT-7'den önce açılmış oturumlarda refresh token yok; kullanıcı bir kez yeniden giriş yapmalı.
                await SignOutAfterFailedRefreshAsync(context, "refresh token yok");
                return null;
            }

            var discoveryEndPoint = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityServerUrl,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false,
                }
            });

            if (discoveryEndPoint.IsError)
            {
                // IdentityServer'a geçici olarak ulaşılamıyor; oturumu kapatma, bir sonraki istekte tekrar denensin.
                _logger.LogError("IdentityServer discovery hatası ({Url}): {Error}", _serviceApiSettings.IdentityServerUrl, discoveryEndPoint.Error);
                return null;
            }

            var token = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                ClientId = _clientSettings.MultiShopWebUIClient.ClientId,
                ClientSecret = _clientSettings.MultiShopWebUIClient.ClientSecret,
                RefreshToken = refreshToken,
                Address = discoveryEndPoint.TokenEndpoint
            });

            if (token.IsError)
            {
                await SignOutAfterFailedRefreshAsync(context, $"{token.Error} {token.ErrorDescription}");
                return null;
            }

            authenticateResult.Properties!.StoreTokens(CreateAuthenticationTokens(token));

            try
            {
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticateResult.Principal!, authenticateResult.Properties);
            }
            catch (InvalidOperationException ex)
            {
                // Yanıt gönderilmeye başlandıysa cookie güncellenemez; yeni token bu istekte yine kullanılır,
                // refresh token tekrar kullanılabilir olduğu için (ReUse) sonraki istekte yeniden yenilenir.
                _logger.LogDebug(ex, "Yenilenen token cookie'ye yazılamadı");
            }

            return token.AccessToken;
        }

        private async Task SignOutAfterFailedRefreshAsync(HttpContext context, string reason)
        {
            _logger.LogInformation("Oturum yenilenemedi ({Reason}); kullanıcının oturumu kapatılıyor", reason);
            try
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (InvalidOperationException)
            {
                // Yanıt başladıysa cookie silinemez; bir sonraki istekte tekrar denenecek.
            }
            // Bu isteğin geri kalanı ziyaretçi olarak devam etsin (Catalog/Comment ziyaretçi token'ına düşer).
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
        }

        private static List<AuthenticationToken> CreateAuthenticationTokens(TokenResponse token) => new()
        {
            new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = token.AccessToken },
            new AuthenticationToken { Name = OpenIdConnectParameterNames.RefreshToken, Value = token.RefreshToken },
            // Standart ad ve kültürden bağımsız (ISO 8601, UTC) biçim; UserAccessTokenService süreyi buradan okur.
            new AuthenticationToken { Name = "expires_at", Value = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture) }
        };

        public async Task<ClaimsPrincipal?> SignIn(SignInDto signInDto)
        {
            var discoveryEndPoint = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityServerUrl,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false,
                }
            });

            if (discoveryEndPoint.IsError)
            {
                _logger.LogError("IdentityServer discovery hatası ({Url}): {Error}", _serviceApiSettings.IdentityServerUrl, discoveryEndPoint.Error);
                return null;
            }

            var passwordTokenRequest = new PasswordTokenRequest
            {
                ClientId = _clientSettings.MultiShopWebUIClient.ClientId,
                ClientSecret = _clientSettings.MultiShopWebUIClient.ClientSecret,
                UserName = signInDto.UserName,
                Password = signInDto.Password,
                Address = discoveryEndPoint.TokenEndpoint
            };

            var token = await _httpClient.RequestPasswordTokenAsync(passwordTokenRequest);

            if (token.IsError)
            {
                _logger.LogWarning("Token alınamadı ({StatusCode}): {Error} - {ErrorDescription}", token.HttpStatusCode, token.Error, token.ErrorDescription);
                return null;
            }

            var userInfoRequest = new UserInfoRequest
            {
                Token = token.AccessToken,
                Address = discoveryEndPoint.UserInfoEndpoint,
            };

            var userValues = await _httpClient.GetUserInfoAsync(userInfoRequest);

            if (userValues.IsError)
            {
                _logger.LogError("Kullanıcı bilgisi alınamadı ({StatusCode}): {Error}", userValues.HttpStatusCode, userValues.Error);
                return null;
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userValues.Claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authenticationProperties = new AuthenticationProperties();

            authenticationProperties.StoreTokens(CreateAuthenticationTokens(token));

            authenticationProperties.IsPersistent = false;

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal, authenticationProperties);

            // HttpContext.User bu istekte henüz güncellenmez; yönlendirme kararı için kullanıcıyı geri ver.
            return claimsPrincipal;
        }

        public async Task<bool> Logout()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
            return true;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}