// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

// bu Config sayfasında yetkilendirme işlemleri yapılıyor.
namespace MultiShop.IdentityServer
{
    public static class Config
    {
        // Her mikroservis için o mikroservise erişimde gerekecek audience (aud) değeri.
        // UserClaims: kullanıcı adına alınan access token'lara "role" claim'i eklenir; servisler rol kontrolünü buna göre yapar.
        public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
        {
            new ApiResource("ResourceCatalog", new[] { JwtClaimTypes.Role })
            {
                Scopes={"CatalogFullPermission","CatalogReadPermission","ContactWritePermission"}
            },
            new ApiResource("ResourceDiscount", new[] { JwtClaimTypes.Role })
            {
                Scopes={"DiscountFullPermission"}
            },
            new ApiResource("ResourceOrder", new[] { JwtClaimTypes.Role })
            {
                Scopes={"OrderFullPermission"}
            },
            new ApiResource("ResourceCargo", new[] { JwtClaimTypes.Role })
            {
                Scopes={"CargoFullPermission"}
            },
            new ApiResource("ResourceBasket", new[] { JwtClaimTypes.Role })
            {
                Scopes={"BasketFullPermission"}
            },
            new ApiResource("ResourceComment", new[] { JwtClaimTypes.Role })
            {
                Scopes={"CommentFullPermission","CommentReadPermission"}
            },
            new ApiResource("ResourcePayment", new[] { JwtClaimTypes.Role })
            {
                Scopes={"PaymentFullPermission"}
            },
            new ApiResource("ResourceImage", new[] { JwtClaimTypes.Role })
            {
                Scopes={"ImageFullPermission"}
            },
            new ApiResource("ResourceOcelot", new[] { JwtClaimTypes.Role })
            {
                Scopes={"OcelotFullPermission"}
            },
            new ApiResource("ResourceMessage", new[] { JwtClaimTypes.Role })
            {
                Scopes={"MessageFullPermission"}
            },
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName, new[] { JwtClaimTypes.Role })
        };

        public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Email(),
            new IdentityResources.Profile(),
            // WebUI kullanıcı bilgisini userinfo ucundan alıyor; rol de oradan gelsin (cookie'deki User.IsInRole için).
            new IdentityResource("roles", "Kullanıcı rolleri", new[] { JwtClaimTypes.Role })
        };

        public static IEnumerable<ApiScope> ApiScopes => new ApiScope[]
        {
            new ApiScope("CatalogFullPermission", "Full authority for catalog operations"),
            new ApiScope("CatalogReadPermission", "Reading authority for catalog operations"),
            // Ziyaretçinin giriş yapmadan iletişim mesajı gönderebilmesi için yalnızca POST /contacts'a izin verir.
            new ApiScope("ContactWritePermission", "Sending contact messages"),
            new ApiScope("DiscountFullPermission", "Full authority for discount operations"),
            new ApiScope("OrderFullPermission", "Full authority for order operations"),
            new ApiScope("CargoFullPermission", "Full authority for cargo operations"),
            new ApiScope("BasketFullPermission", "Full authority for basket operations"),
            new ApiScope("CommentFullPermission", "Full authority for comment operations"),
            new ApiScope("CommentReadPermission", "Reading authority for comment operations"),
            new ApiScope("PaymentFullPermission", "Full authority for payment operations"),
            new ApiScope("ImageFullPermission", "Full authority for image operations"),
            new ApiScope("OcelotFullPermission", "Full authority for ocelot operations"),
            new ApiScope("MessageFullPermission", "Full authority for message operations"),
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
        };

        // İki client var:
        // - Visitor: giriş yapmamış ziyaretçi adına WebUI'ın aldığı token; yalnızca okuma (+ iletişim mesajı).
        // - WebUI: giriş yapan kullanıcı adına alınan token. Admin/müşteri ayrımı client ile değil "role" claim'iyle yapılır.
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            var accessTokenLifetime = configuration.GetValue("TokenLifetimes:AccessTokenSeconds", 3600);

            return new Client[]
            {
                new Client
                {
                    ClientId = "MultiShopVisitorId",
                    ClientName = "Multi Shop Visitor",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret(RequiredSecret(configuration, "ClientSecrets:Visitor").Sha256()) },
                    AllowedScopes =
                    {
                        "CatalogReadPermission",
                        "ContactWritePermission",
                        "CommentReadPermission",
                        "OcelotFullPermission"
                    },
                    AccessTokenLifetime = accessTokenLifetime
                },

                new Client
                {
                    ClientId = "MultiShopWebUIId",
                    ClientName = "Multi Shop WebUI",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret(RequiredSecret(configuration, "ClientSecrets:WebUI").Sha256()) },
                    AllowedScopes =
                    {
                        "CatalogFullPermission", "CatalogReadPermission", "ContactWritePermission",
                        "BasketFullPermission", "CommentFullPermission", "CommentReadPermission",
                        "PaymentFullPermission", "ImageFullPermission", "DiscountFullPermission",
                        "OrderFullPermission", "MessageFullPermission", "CargoFullPermission",
                        "OcelotFullPermission",
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "roles"
                    },
                    AccessTokenLifetime = accessTokenLifetime,

                    // Refresh token: access token süresi dolunca kullanıcı yeniden giriş yapmadan yeni token alınır.
                    // Kullanıldıkça süresi uzar (5 gün hareketsizlikte, en geç 30 günde biter).
                    // ReUse: aynı anda gelen iki istek aynı refresh token'ı kullanınca biri reddedilip kullanıcı düşmesin;
                    // refresh token tarayıcıya hiç gitmiyor, WebUI'ın HttpOnly cookie'sinde sunucu tarafında tutuluyor.
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = (int)TimeSpan.FromDays(5).TotalSeconds,
                    AbsoluteRefreshTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                    // Rol değişince yenilenen token'a yeni rol yansısın.
                    UpdateAccessTokenClaimsOnRefresh = true
                }
            };
        }

        private static string RequiredSecret(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"'{key}' ayarı eksik. user-secrets'a ekleyin.");
            return value;
        }
    }
}
