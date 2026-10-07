namespace MultiShop.WebUI.Settings
{
    public class ClientSettings
    {
        // Giriş yapmamış ziyaretçi adına token (yalnızca okuma + iletişim mesajı).
        public Client MultiShopVisitorClient { get; set; }

        // Giriş yapan kullanıcı adına token; admin/müşteri ayrımı token'daki "role" claim'iyle yapılır.
        public Client MultiShopWebUIClient { get; set; }
    }
    
    public class Client
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
