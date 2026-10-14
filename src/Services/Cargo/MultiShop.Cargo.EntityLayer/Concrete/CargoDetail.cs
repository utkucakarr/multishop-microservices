namespace MultiShop.Cargo.EntityLayer.Concrete
{
    /// <summary>
    /// Bir siparişin gönderisi (KT-13). Order saga'sının <c>CreateShipment</c> komutuyla açılır; alıcı bilgisi siparişteki
    /// adresin kopyasıdır (sonradan değişen adres kargoyu etkilemez). Gönderici her zaman mağaza.
    /// </summary>
    public class CargoDetail
    {
        public int CargoDetailId { get; set; }

        /// <summary>Siparişin servisler arası kimliği; bir siparişin tek gönderisi olur (unique).</summary>
        public Guid OrderNumber { get; set; }

        /// <summary>Order'daki sipariş numarası (ekranlar için). KT-13 öncesi kayıtlarda 0.</summary>
        public int OrderingId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string ReceiverName { get; set; } = string.Empty;

        public string ReceiverEmail { get; set; } = string.Empty;

        public string ReceiverPhone { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string? ZipCode { get; set; }

        /// <summary>Takip numarası (unique). Hareketler bu gönderiye bağlıdır (CAR-01).</summary>
        public string Barcode { get; set; } = string.Empty;

        public ShipmentStatus Status { get; set; }

        /// <summary>Gönderi açılırken varsayılan firma atanır; hiç firma yoksa boş kalır, kargoya verilemez.</summary>
        public int? CargoCompanyId { get; set; }

        public CargoCompany? CargoCompany { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DispatchedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public List<CargoOperation> Operations { get; set; } = new();
    }

    public enum ShipmentStatus
    {
        Preparing = 0,  // Sipariş ödendi, gönderi hazırlanıyor
        Shipped = 1,    // Kargo firmasına verildi
        Delivered = 2   // Müşteriye teslim edildi
    }
}
