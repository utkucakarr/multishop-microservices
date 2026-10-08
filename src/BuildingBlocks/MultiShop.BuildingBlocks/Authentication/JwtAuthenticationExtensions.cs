using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MultiShop.BuildingBlocks.Authentication;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// IdentityServer'ın (<c>IdentityServerUrl</c>) verdiği JWT'leri doğrular.
    /// </summary>
    /// <param name="audience">Token'ın <c>aud</c> değeri, ör. <c>ResourceCatalog</c>.</param>
    public static AuthenticationBuilder AddMultiShopJwtAuthentication(this WebApplicationBuilder builder, string audience)
    {
        var authority = builder.Configuration["IdentityServerUrl"]
            ?? throw new InvalidOperationException("'IdentityServerUrl' ayarı eksik.");

        return builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                // Geliştirmede IdentityServer http üzerinden çalışıyor.
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                // .NET 8: claim adları token'daki gibi kalsın ("sub" → ClaimTypes.NameIdentifier'a çevrilmesin).
                options.MapInboundClaims = false;
                // Claim adları çevrilmediği için rol ve ad da token'daki adlarıyla okunmalı;
                // aksi halde User.IsInRole / RequireRole uzun Microsoft claim adını arar ve "role" claim'ini hiç görmez.
                options.TokenValidationParameters.RoleClaimType = "role";
                options.TokenValidationParameters.NameClaimType = "name";
            });
    }
}
