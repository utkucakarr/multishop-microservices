using IdentityModel;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.Interfaces;
using System.Security.Claims;

namespace MultiShop.WebUI.Services.Concrete
{
    // Cookie, IdentityService.SignIn'de userinfo claim'leriyle (sub, name, email, given_name, family_name, role) oluşturulur.
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public bool IsAdmin => Principal?.IsInRole(MultiShopRoles.Admin) == true;

        public string? UserId => Find(JwtClaimTypes.Subject);

        public string? UserName => Find(JwtClaimTypes.Name);

        public string? Email => Find(JwtClaimTypes.Email);

        public string? FirstName => Find(JwtClaimTypes.GivenName);

        public string? LastName => Find(JwtClaimTypes.FamilyName);

        public string DisplayName
        {
            get
            {
                var fullName = $"{FirstName} {LastName}".Trim();
                return string.IsNullOrEmpty(fullName) ? UserName ?? string.Empty : fullName;
            }
        }

        private string? Find(string type) => Principal?.FindFirst(type)?.Value;
    }
}
