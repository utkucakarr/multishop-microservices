using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Handlers
{
    /// <summary>
    /// Herkese açık sayfalarda da kullanılan servisler (Catalog, Comment) için:
    /// kullanıcı giriş yaptıysa kendi token'ı (ör. admin ürün eklerken token'da role=Admin olur),
    /// giriş yapmadıysa yalnızca okuma yetkili ziyaretçi (Visitor) token'ı gönderilir.
    /// </summary>
    public class UserOrVisitorTokenHandler : ResourceOwnerPasswordTokenHandler
    {
        private readonly IClientCredentialTokenService _clientCredentialTokenService;

        public UserOrVisitorTokenHandler(IUserAccessTokenService userAccessTokenService, IClientCredentialTokenService clientCredentialTokenService)
            : base(userAccessTokenService)
        {
            _clientCredentialTokenService = clientCredentialTokenService;
        }

        protected override async Task<string?> GetFallbackTokenAsync()
            => await _clientCredentialTokenService.GetToken();
    }
}
