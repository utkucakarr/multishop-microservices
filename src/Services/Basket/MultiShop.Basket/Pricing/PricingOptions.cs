namespace MultiShop.Basket.Pricing
{
    /// <summary><c>Pricing</c> ayar bölümü: KDV oranı ve kargo ücreti tek yerde, kodda sabit değil.</summary>
    public class PricingOptions
    {
        public const string SectionName = "Pricing";

        /// <summary>İndirimli tutara eklenen KDV yüzdesi.</summary>
        public decimal TaxRatePercent { get; set; } = 10;

        /// <summary>Sabit kargo ücreti; boş sepette alınmaz.</summary>
        public decimal ShippingFee { get; set; } = 20;
    }
}
