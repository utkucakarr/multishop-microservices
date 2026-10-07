namespace MultiShop.BuildingBlocks.Authentication;

/// <summary>İsteği yapan kullanıcının token'dan okunan bilgileri.</summary>
public interface ICurrentUserService
{
    /// <summary>Token'daki <c>sub</c> claim'i; anonim istekte ya da client-credentials token'ında <c>null</c>.</summary>
    string? UserId { get; }

    bool IsAuthenticated { get; }

    /// <summary>Kullanıcı ID'si yoksa 401'e çevrilen <see cref="UnauthorizedAccessException"/> fırlatır.</summary>
    string GetRequiredUserId();
}
