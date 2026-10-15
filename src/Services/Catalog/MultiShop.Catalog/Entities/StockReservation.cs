using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MultiShop.Catalog.Entities
{
    /// <summary>
    /// Bir siparişin stok rezervasyonu (KT-12). Kimliği siparişin OrderNumber'ı: aynı sipariş için ikinci rezervasyon
    /// açılamaz; kesinleştirme ve serbest bırakma hangi ürünlerden kaç adet ayrıldığını buradan bilir.
    /// </summary>
    public class StockReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid OrderNumber { get; set; }

        public List<StockReservationItem> Items { get; set; } = new();

        [BsonRepresentation(BsonType.String)]
        public StockReservationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class StockReservationItem
    {
        public string ProductId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }

    public enum StockReservationStatus
    {
        /// <summary>Stok ayrıldı, ödeme bekleniyor.</summary>
        Reserved,
        /// <summary>Ödeme alındı, eldeki stoktan düşüldü.</summary>
        Committed,
        /// <summary>Sipariş iptal edildi, ayrılan stok serbest bırakıldı.</summary>
        Released,
    }
}
