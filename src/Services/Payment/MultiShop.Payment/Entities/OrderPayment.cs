using Microsoft.EntityFrameworkCore;

namespace MultiShop.Payment.Entities
{
    /// <summary>
    /// Bir siparişin ödemesi (OrderPayments): Order saga'sı stok ayırınca açılır (OrderAwaitingPayment), tutarı oradan gelir.
    /// Kullanıcı yalnızca kart bilgisini gönderir; tahsil edilen tutar bu kayıttadır, istekte değil.
    /// </summary>
    public class OrderPayment
    {
        public int Id { get; set; }

        public Guid OrderNumber { get; set; }

        /// <summary>Order'daki sipariş numarası; bankaya giden sipariş kimliğinde kullanılır. Ödeme beklemeden iptal edilen siparişte 0.</summary>
        public int OrderingId { get; set; }

        public string UserId { get; set; } = null!;

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public OrderPaymentStatus Status { get; set; }

        public int FailedAttempts { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }

    public enum OrderPaymentStatus
    {
        AwaitingPayment = 0,  // Kart bekleniyor
        Processing = 1,       // Kart bankaya gönderildi; bu sırada ikinci bir deneme kabul edilmez
        Succeeded = 2,        // Ödeme alındı
        Failed = 3,           // Deneme hakkı bitti (PaymentPolicy.MaxAttempts)
        Cancelled = 4         // Sipariş iptal edildi
    }
}
