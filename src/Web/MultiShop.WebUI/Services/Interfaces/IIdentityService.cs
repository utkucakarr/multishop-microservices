using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using System.Security.Claims;

namespace MultiShop.WebUI.Services.Interfaces
{
    public interface IIdentityService
    {
        /// <summary>
        /// Kullanıcıyı IdentityServer'da doğrular ve cookie ile oturum açar.
        /// Giriş yapan kullanıcıyı (rolleriyle birlikte) döner; giriş başarısızsa <c>null</c>.
        /// </summary>
        Task<ClaimsPrincipal?> SignIn(SignInDto signInDto);

        Task<bool> Logout();

        bool IsAuthenticated();

        /// <summary>
        /// Cookie'deki refresh token ile yeni access token alır ve cookie'yi günceller.
        /// Yenilenemezse (refresh token süresi dolmuş, IdentityServer yeniden başlamış vb.) kullanıcının
        /// oturumunu kapatır ve <c>null</c> döner.
        /// </summary>
        Task<string?> RefreshAccessTokenAsync();
    }
}
