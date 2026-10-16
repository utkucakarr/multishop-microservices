namespace MultiShop.WebUI.Services.Interfaces
{
    public interface IClientCredentialTokenService
    {
        /// <summary>Ziyaretçi (Visitor) token'ı; IdentityServer'a ulaşılamazsa null (istek token'sız gider, gateway 401 döner).</summary>
        Task<string?> GetToken();
    }
}
