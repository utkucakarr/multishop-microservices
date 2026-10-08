using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MultiShop.BuildingBlocks.Authentication;

/// <summary>Servislerin <c>[Authorize(Policy = ...)]</c> ile kullandığı ortak kural adları.</summary>
public static class MultiShopPolicies
{
    /// <summary>Okuma: okuma scope'u veya tam yetki scope'u. Ziyaretçi (Visitor) token'ı da geçer.</summary>
    public const string Read = "Read";

    /// <summary>Giriş yapmış kullanıcı: token'da kullanıcı (<c>sub</c>) ve tam yetki scope'u var.</summary>
    public const string Authenticated = "Authenticated";

    /// <summary>Yönetici: <c>role = Admin</c> ve tam yetki scope'u.</summary>
    public const string Admin = "Admin";
}

/// <summary>IdentityServer'daki rol adları (bkz. IdentitySeeder).</summary>
public static class MultiShopRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}

public static class AuthorizationExtensions
{
    /// <summary>
    /// <see cref="MultiShopPolicies"/> kurallarını bu servisin scope adlarıyla tanımlar.
    /// Scope = uygulamanın (client) neye erişebileceği; rol = kullanıcının ne yapabileceği. Kurallar ikisine birden bakar.
    /// </summary>
    /// <param name="fullScope">Tam yetki scope'u, ör. <c>CatalogFullPermission</c>.</param>
    /// <param name="readScope">Okuma scope'u (varsa), ör. <c>CatalogReadPermission</c>.</param>
    public static AuthorizationBuilder AddMultiShopAuthorization(this IServiceCollection services, string fullScope, string? readScope = null)
    {
        string[] readScopes = readScope is null ? [fullScope] : [readScope, fullScope];

        return services.AddAuthorizationBuilder()
            .AddPolicy(MultiShopPolicies.Read, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => HasAnyScope(context.User, readScopes)))
            .AddPolicy(MultiShopPolicies.Authenticated, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("sub")
                .RequireAssertion(context => HasAnyScope(context.User, fullScope)))
            .AddPolicy(MultiShopPolicies.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(MultiShopRoles.Admin)
                .RequireAssertion(context => HasAnyScope(context.User, fullScope)));
    }

    /// <summary>
    /// Token'da verilen scope'lardan en az biri var mı? IdentityServer scope'ları JSON dizisi olarak yazar
    /// (her biri ayrı <c>scope</c> claim'i olur); boşlukla ayrılmış tek değer gelirse o da desteklenir.
    /// </summary>
    public static bool HasAnyScope(ClaimsPrincipal user, params string[] scopes)
        => user.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Any(scope => scopes.Contains(scope, StringComparer.Ordinal));
}
