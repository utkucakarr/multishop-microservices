using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.IntegrationTests.Infrastructure
{
    /// <summary>Testte kim olarak istek atılacağı; IdentityServer token'larındaki claim'lerin karşılığı.</summary>
    public enum TestUser
    {
        Anonymous,
        /// <summary>Giriş yapmamış ziyaretçi (client credentials): yalnızca okuma scope'u.</summary>
        Visitor,
        Customer,
        Admin
    }

    /// <summary>
    /// JWT yerine <c>X-Test-User</c> başlığına göre kullanıcı oluşturur. Yetki kuralları (policy'ler) değişmez:
    /// scope ve rol kontrolleri gerçek token'daki gibi çalışır.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string HeaderName = "X-Test-User";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderName, out var value) || !Enum.TryParse<TestUser>(value, out var user) || user == TestUser.Anonymous)
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = user switch
            {
                TestUser.Visitor => new List<Claim> { new("client_id", "MultiShopVisitorId"), new("scope", "CatalogReadPermission") },
                TestUser.Customer => new List<Claim> { new("sub", "customer-1"), new("scope", "CatalogFullPermission"), new("role", MultiShopRoles.Customer) },
                _ => new List<Claim> { new("sub", "admin-1"), new("scope", "CatalogFullPermission"), new("role", MultiShopRoles.Admin) },
            };

            var identity = new ClaimsIdentity(claims, SchemeName, nameType: "name", roleType: "role");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
    }
}
