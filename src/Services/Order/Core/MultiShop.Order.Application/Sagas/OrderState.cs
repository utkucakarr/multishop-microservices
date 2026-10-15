using MassTransit;

namespace MultiShop.Order.Application.Sagas
{
    /// <summary>
    /// Sipariş saga'sının kaydı (OrderStates tablosu). Siparişin kendisi Orderings tablosundadır; burada saga'nın
    /// sonraki mesajlarda ihtiyaç duyduğu bilgiler ve durum tutulur. Kayıt silinmez: iptal edilmiş siparişe geç gelen
    /// bir "stok ayrıldı" mesajında ayrılan stoğu geri bırakabilmek için saga'nın siparişi hatırlaması gerekir.
    /// </summary>
    public class OrderState : SagaStateMachineInstance
    {
        /// <summary>Sipariş numarası (<c>OrderNumber</c>); tüm sipariş mesajları bununla eşleşir.</summary>
        public Guid CorrelationId { get; set; }

        /// <summary>Saga durumunun adı: Pending, AwaitingPayment, Paid, Cancelled (OrderStatus ile aynı adlar).</summary>
        public string CurrentState { get; set; } = null!;

        public int OrderingId { get; set; }

        public string UserId { get; set; } = null!;

        /// <summary>Tahsil edilecek tutar (Basket'in hesapladığı genel toplam).</summary>
        public decimal TotalPrice { get; set; }

        /// <summary>Başarısız ödeme denemesi sayısı; sınıra ulaşınca sipariş iptal edilir.</summary>
        public int PaymentAttempts { get; set; }

        /// <summary>İptal sebebi ya da son başarısız ödeme denemesinin sebebi.</summary>
        public string? FailReason { get; set; }

        /// <summary>Kargo takip numarası ve firması (gönderi kargoya verilince).</summary>
        public string? TrackingNumber { get; set; }

        public string? CargoCompanyName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>İyimser eşzamanlılık: aynı siparişin iki mesajı aynı anda işlenirse biri çakışma alır ve yeniden denenir.</summary>
        public byte[] RowVersion { get; set; } = null!;
    }
}
