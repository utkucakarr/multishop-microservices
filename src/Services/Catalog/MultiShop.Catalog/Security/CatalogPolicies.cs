using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.Security
{
    /// <summary>Catalog'a özel yetki kuralları (ortak kurallar: <see cref="MultiShopPolicies"/>).</summary>
    public static class CatalogPolicies
    {
        /// <summary>İletişim mesajı gönderme: giriş yapmamış ziyaretçinin token'ı (ContactWritePermission) da yeterli.</summary>
        public const string ContactWrite = "ContactWrite";
    }
}
