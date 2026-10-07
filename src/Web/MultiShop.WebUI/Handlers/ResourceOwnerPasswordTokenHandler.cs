using MultiShop.WebUI.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;

namespace MultiShop.WebUI.Handlers
{
    /// <summary>
    /// İsteğe giriş yapmış kullanıcının access token'ını ekler. Token süresi dolmak üzereyse önce yenilenir;
    /// servis yine de 401 dönerse token bir kez yenilenip istek tekrar gönderilir.
    /// </summary>
    public class ResourceOwnerPasswordTokenHandler : DelegatingHandler
    {
        private readonly IUserAccessTokenService _userAccessTokenService;

        public ResourceOwnerPasswordTokenHandler(IUserAccessTokenService userAccessTokenService)
        {
            _userAccessTokenService = userAccessTokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userToken = await _userAccessTokenService.GetAccessTokenAsync();
            SetBearerToken(request, userToken ?? await GetFallbackTokenAsync());

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && userToken is not null)
            {
                // Token süresinden önce geçersiz olmuş olabilir (ör. IdentityServer yeniden başlatıldı).
                var refreshedToken = await _userAccessTokenService.GetAccessTokenAsync(forceRefresh: true);
                if (refreshedToken is not null && refreshedToken != userToken)
                {
                    response.Dispose();
                    SetBearerToken(request, refreshedToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        /// <summary>Kullanıcı giriş yapmamışsa kullanılacak token; varsayılan olarak yok.</summary>
        protected virtual Task<string?> GetFallbackTokenAsync() => Task.FromResult<string?>(null);

        private static void SetBearerToken(HttpRequestMessage request, string? token)
        {
            request.Headers.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
