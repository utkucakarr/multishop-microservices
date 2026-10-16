using IdentityModel;
using IdentityServer4.AspNetIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MultiShop.IdentityServer.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiShop.IdentityServer.Services
{
    /// <summary>
    /// Varsayılan ASP.NET Identity profil servisine ad, soyad ve e-postayı ekler (given_name, family_name, email).
    /// WebUI bunları userinfo ucundan alıp cookie'ye yazar; böylece her sayfada Identity'ye "kullanıcı bilgisi" isteği gerekmez.
    /// Claim'ler yalnızca istenen scope'a göre (profile, email) verilir; access token'a girmez.
    /// </summary>
    public class MultiShopProfileService : ProfileService<ApplicationUser>
    {
        public MultiShopProfileService(
            UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            ILogger<ProfileService<ApplicationUser>> logger)
            : base(userManager, claimsFactory, logger)
        {
        }

        protected override async Task<ClaimsPrincipal> GetUserClaimsAsync(ApplicationUser user)
        {
            var principal = await base.GetUserClaimsAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            AddIfMissing(identity, JwtClaimTypes.GivenName, user.Name);
            AddIfMissing(identity, JwtClaimTypes.FamilyName, user.Surname);
            // ASP.NET Identity e-postayı uzun URI claim tipiyle üretir; userinfo yalnızca "email" tipini döndürür.
            AddIfMissing(identity, JwtClaimTypes.Email, user.Email);

            return principal;
        }

        private static void AddIfMissing(ClaimsIdentity identity, string type, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !identity.HasClaim(c => c.Type == type))
                identity.AddClaim(new Claim(type, value));
        }
    }
}
