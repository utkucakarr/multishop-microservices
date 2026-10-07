using MultiShop.DtoLayer.IdentityDtos.LoginDtos;

namespace MultiShop.WebUI.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> SignIn(SignInDto signInDto);

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
