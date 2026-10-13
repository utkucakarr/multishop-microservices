using Microsoft.Extensions.Options;

namespace MultiShop.Basket.Pricing
{
    /// <summary>Sepet satırının fiyatlamaya giren kısmı.</summary>
    public record PricingLine(decimal UnitPrice, int Quantity);

    public record BasketTotals(
        decimal Subtotal,
        int DiscountRate,
        decimal DiscountAmount,
        decimal TaxRatePercent,
        decimal TaxAmount,
        decimal ShippingFee,
        decimal GrandTotal);

    /// <summary>
    /// Sepet toplamının TEK formülü (sepet sayfası, sipariş özeti ve tahsil edilen tutar aynı sonucu kullanır):
    /// indirim = ara toplam × oran; KDV = (ara toplam − indirim) × KDV oranı; genel toplam = ara toplam − indirim + KDV + kargo.
    /// Her tutar kuruşa yuvarlanır (0,005 yukarı).
    /// </summary>
    public class PricingCalculator
    {
        private readonly PricingOptions _options;

        public PricingCalculator(IOptions<PricingOptions> options)
        {
            _options = options.Value;
        }

        public static decimal LineTotal(PricingLine line) => Round(line.UnitPrice * line.Quantity);

        public BasketTotals Calculate(IReadOnlyCollection<PricingLine> lines, int discountRate)
        {
            var subtotal = lines.Sum(LineTotal);
            var discountAmount = Round(subtotal * discountRate / 100);
            var taxAmount = Round((subtotal - discountAmount) * _options.TaxRatePercent / 100);
            var shippingFee = lines.Count == 0 ? 0 : _options.ShippingFee;
            var grandTotal = subtotal - discountAmount + taxAmount + shippingFee;

            return new BasketTotals(subtotal, discountRate, discountAmount, _options.TaxRatePercent, taxAmount, shippingFee, grandTotal);
        }

        private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
