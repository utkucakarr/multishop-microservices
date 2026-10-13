namespace MultiShop.DtoLayer.BasketDtos
{
    /// <summary>
    /// Basket servisinin fiyatladığı sepet. WebUI tutar hesaplamaz, yalnızca bunu gösterir; fiyatlar Catalog'dan,
    /// indirim Discount'tan gelir, toplamlar Basket'te tek formülle hesaplanır.
    /// </summary>
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

        /// <summary>Sipariş toplamı ve tahsil edilen tutar.</summary>
        public decimal GrandTotal { get; set; }

        /// <summary>Sepette yapılan düzeltmeler (satıştan kalkan ürün, süresi dolan kupon); kullanıcıya gösterilir.</summary>
        public List<string> Warnings { get; set; } = new();
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
