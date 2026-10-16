namespace MultiShop.WebUI.Services.Interfaces
{
    /// <summary>
    /// Giriş yapan kullanıcının cookie'deki claim'lerden okunan bilgileri (ağ isteği yapmaz).
    /// Ad, soyad ve e-posta girişte IdentityServer'ın userinfo ucundan gelir; eski oturumlarda boş olabilir.
    /// Güncel profil gereken yerde (profil sayfası) <see cref="IUserService.GetUserInfo"/> kullanılır.
    /// </summary>
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }

        bool IsAdmin { get; }

        string? UserId { get; }

        string? UserName { get; }

        string? Email { get; }

        string? FirstName { get; }

        string? LastName { get; }

        /// <summary>"Ad Soyad"; ikisi de yoksa kullanıcı adı.</summary>
        string DisplayName { get; }
    }
}
