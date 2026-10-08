using Microsoft.AspNetCore.Http;

namespace MultiShop.BuildingBlocks.Authentication;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    // MapInboundClaims = false olduğu için claim adı token'daki gibi "sub" kalır.
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => httpContextAccessor.HttpContext?.User.IsInRole(MultiShopRoles.Admin) ?? false;

    public string GetRequiredUserId()
        => UserId ?? throw new UnauthorizedAccessException("Token'da kullanıcı kimliği (sub) bulunamadı.");
}
