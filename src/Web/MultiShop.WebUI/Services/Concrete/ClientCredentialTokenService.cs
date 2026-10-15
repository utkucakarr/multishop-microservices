using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Settings;

namespace MultiShop.WebUI.Services.Concrete
{
    public class ClientCredentialTokenService : IClientCredentialTokenService
    {
        private readonly ServiceApiSettings _serviceApiSettings;
        private readonly HttpClient _httpClient;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly ClientSettings _clientSettings;
        private readonly ILogger<ClientCredentialTokenService> _logger;

        public ClientCredentialTokenService(IOptions<ServiceApiSettings> serviceApiSettings, HttpClient httpClient, IClientAccessTokenCache clientAccessTokenCache, IOptions<ClientSettings> clientSettings, ILogger<ClientCredentialTokenService> logger)
        {
            _logger = logger;
            _serviceApiSettings = serviceApiSettings.Value;
            _httpClient = httpClient;
            _clientAccessTokenCache = clientAccessTokenCache;
            _clientSettings = clientSettings.Value;
        }

        public async Task<string?> GetToken()
       {
            var token1 = await _clientAccessTokenCache.GetAsync("multishoptoken");
            if (token1 != null)
            {
                return token1.AccessToken;
            }
            var discoveryEndPoint = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityServerUrl,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false
                }
            });

            if (discoveryEndPoint.IsError)
            {
                _logger.LogError("IdentityServer discovery hatası ({Url}): {Error}", _serviceApiSettings.IdentityServerUrl, discoveryEndPoint.Error);
                return null;
            }

            var clientCredentialTokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = _clientSettings.MultiShopVisitorClient.ClientId,
                ClientSecret = _clientSettings.MultiShopVisitorClient.ClientSecret,
                Address = discoveryEndPoint.TokenEndpoint
            };
            var token2 = await _httpClient.RequestClientCredentialsTokenAsync(clientCredentialTokenRequest);
            if (token2.IsError)
            {
                // Başarısız sonuç önbelleğe yazılmaz; bir sonraki istekte yeniden denenir.
                _logger.LogError("Ziyaretçi token'ı alınamadı ({StatusCode}): {Error} - {ErrorDescription}", token2.HttpStatusCode, token2.Error, token2.ErrorDescription);
                return null;
            }
            await _clientAccessTokenCache.SetAsync("multishoptoken", token2.AccessToken, token2.ExpiresIn);
            return token2.AccessToken;
        }
    }
}
