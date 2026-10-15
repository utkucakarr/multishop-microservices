namespace MultiShop.Basket.Clients
{
    /// <summary><c>ServiceUrls</c> ayar bölümü: Basket'in doğrudan (gateway'siz) çağırdığı servislerin adresleri.</summary>
    public class ServiceUrls
    {
        public const string SectionName = "ServiceUrls";

        public string Catalog { get; set; } = string.Empty;

        public string Discount { get; set; } = string.Empty;

        public string Order { get; set; } = string.Empty;
    }
}
