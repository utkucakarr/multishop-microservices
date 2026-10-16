using Microsoft.Extensions.Options;
using MultiShop.Basket.Pricing;

namespace MultiShop.Basket.UnitTests.Pricing
{
    // Sepet sayfası, sipariş özeti ve tahsil edilen tutar aynı formülü kullanır (KT-11): formül değişirse bu testler kırılır.
    public class PricingCalculatorTests
    {
        private static PricingCalculator CreateCalculator(decimal taxRatePercent = 10, decimal shippingFee = 20)
            => new(Options.Create(new PricingOptions { TaxRatePercent = taxRatePercent, ShippingFee = shippingFee }));

        [Fact]
        public void Empty_basket_has_no_shipping_fee_and_zero_total()
        {
            var totals = CreateCalculator().Calculate(Array.Empty<PricingLine>(), discountRate: 0);

            totals.Subtotal.Should().Be(0);
            totals.ShippingFee.Should().Be(0);
            totals.GrandTotal.Should().Be(0);
        }

        [Fact]
        public void Without_discount_total_is_subtotal_plus_tax_plus_shipping()
        {
            var lines = new[] { new PricingLine(UnitPrice: 50m, Quantity: 2) };

            var totals = CreateCalculator().Calculate(lines, discountRate: 0);

            totals.Subtotal.Should().Be(100m);
            totals.DiscountAmount.Should().Be(0m);
            totals.TaxAmount.Should().Be(10m);
            totals.ShippingFee.Should().Be(20m);
            totals.GrandTotal.Should().Be(130m);
        }

        [Fact]
        public void Tax_is_calculated_on_the_discounted_amount()
        {
            var lines = new[] { new PricingLine(UnitPrice: 50m, Quantity: 2) };

            var totals = CreateCalculator().Calculate(lines, discountRate: 10);

            totals.DiscountRate.Should().Be(10);
            totals.DiscountAmount.Should().Be(10m);
            totals.TaxAmount.Should().Be(9m, "KDV indirimli tutar (90) üzerinden alınır");
            totals.GrandTotal.Should().Be(119m);
        }

        [Fact]
        public void Line_totals_are_rounded_to_cents_before_summing()
        {
            // Her satır ayrı yuvarlanır: 10,005 → 10,01; iki satır 20,02 eder (toplamı yuvarlamak 20,01 verirdi).
            var lines = new[] { new PricingLine(10.005m, 1), new PricingLine(10.005m, 1) };

            var totals = CreateCalculator().Calculate(lines, discountRate: 0);

            totals.Subtotal.Should().Be(20.02m);
        }

        [Theory]
        [InlineData(0.335, 1, 0.34)]
        [InlineData(0.125, 1, 0.13)]
        [InlineData(19.99, 3, 59.97)]
        public void Line_total_rounds_half_away_from_zero(decimal unitPrice, int quantity, decimal expected)
        {
            PricingCalculator.LineTotal(new PricingLine(unitPrice, quantity)).Should().Be(expected);
        }

        [Fact]
        public void Discount_and_tax_amounts_are_rounded_to_cents()
        {
            // 33,33 × %15 = 4,9995 → 5,00; (33,33 − 5,00) × %10 = 2,833 → 2,83
            var lines = new[] { new PricingLine(33.33m, 1) };

            var totals = CreateCalculator().Calculate(lines, discountRate: 15);

            totals.DiscountAmount.Should().Be(5.00m);
            totals.TaxAmount.Should().Be(2.83m);
            totals.GrandTotal.Should().Be(33.33m - 5.00m + 2.83m + 20m);
        }

        [Fact]
        public void Tax_rate_and_shipping_fee_come_from_options()
        {
            var lines = new[] { new PricingLine(100m, 1) };

            var totals = CreateCalculator(taxRatePercent: 20, shippingFee: 0).Calculate(lines, discountRate: 0);

            totals.TaxRatePercent.Should().Be(20);
            totals.TaxAmount.Should().Be(20m);
            totals.ShippingFee.Should().Be(0m);
            totals.GrandTotal.Should().Be(120m);
        }
    }
}
