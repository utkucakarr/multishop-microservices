using MultiShop.Order.Domain.Enums;

namespace MultiShop.Order.Domain.Entities
{
    public class Ordering
    {
        public int OrderingId { get; set; }

        // Siparişin servisler arası kimliği: Basket üretir, saga'nın CorrelationId'si, tüm sipariş mesajlarında aynı (KT-12).
        public Guid OrderNumber { get; set; }

        public string UserId { get; set; }

        // Teslimat adresi (Addresses tablosu). Adres sonradan silinebileceği için FK yok; KT-12 öncesi siparişlerde 0.
        public int AddressId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        // Saga'nın durumu; yalnızca saga değiştirir (API'den güncellenmez)
        public OrderStatus Status { get; set; }

        // İptal sebebi ya da son başarısız ödeme denemesinin sebebi
        public string? FailMessage { get; set; }

        // Kargo takip numarası ve firması; sipariş kargoya verilince saga yazar (KT-13)
        public string? TrackingNumber { get; set; }

        public string? CargoCompanyName { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
