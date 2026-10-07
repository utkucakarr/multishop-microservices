namespace MultiShop.WebUI.Services.Interfaces
{
    public interface IUserAccessTokenService
    {
        /// <summary>
        /// Giriş yapmış kullanıcının geçerli access token'ı; süresi dolmak üzereyse önce yenilenir.
        /// Kullanıcı giriş yapmamışsa ya da oturum yenilenemiyorsa <c>null</c>.
        /// </summary>
        Task<string?> GetAccessTokenAsync(bool forceRefresh = false);
    }
}
