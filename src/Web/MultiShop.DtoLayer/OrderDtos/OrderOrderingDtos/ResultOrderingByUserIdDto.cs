namespace MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos
{
    /// <summary>Sipariş ve durumu (Order API: <c>mine</c>, <c>by-number</c>, admin listesi).</summary>
    public class ResultOrderingByUserIdDto
    {
        public int OrderingId { get; set; }

        public Guid OrderNumber { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int AddressId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        /// <summary>Pending, AwaitingPayment, Paid, Shipped, Completed, Cancelled (Order API metin olarak döner).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>İptal sebebi ya da son başarısız ödeme denemesinin sebebi.</summary>
        public string? FailMessage { get; set; }

        /// <summary>Kargo takip numarası ve firması (sipariş kargoya verildiyse).</summary>
        public string? TrackingNumber { get; set; }

        public string? CargoCompanyName { get; set; }
    }
}
