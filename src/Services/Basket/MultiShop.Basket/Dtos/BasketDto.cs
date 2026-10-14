namespace MultiShop.Basket.Dtos
{
    /// <summary>Fiyatlanmış sepet: satır fiyatları Catalog'dan, indirim Discount'tan, toplamlar PricingCalculator'dan.</summary>
    public class BasketDto
    {
        public List<BasketLineDto> Items { get; set; } = new();

        public string? CouponCode { get; set; }

        public int DiscountRate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxRatePercent { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingFee { get; set; }

        /// <summary>Tahsil edilecek tutar.</summary>
        public decimal GrandTotal { get; set; }

        /// <summary>Okuma sırasında sepette yapılan düzeltmeler (satıştan kalkan ürün, süresi dolan kupon).</summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>Sepet onayının sonucu: sipariş numarası. Sipariş arka planda oluşur, durumu Order'dan izlenir.</summary>
    public class CheckoutResultDto
    {
        public Guid OrderNumber { get; set; }
    }

    public class BasketLineDto
    {
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}
