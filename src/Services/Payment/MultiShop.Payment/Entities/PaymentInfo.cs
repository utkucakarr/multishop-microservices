using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MultiShop.Payment.Entities
{
    /// <summary>
    /// Bir ödeme denemesinin kaydı (PaymentInfos). Kartın yalnızca son 4 hanesi ve bankanın kullanıcıya gösterilebilir
    /// mesajı saklanır; tam kart numarası, CVV ve ham banka yanıtı hiçbir yere yazılmaz (G-10).
    /// </summary>
    public class PaymentInfo
    {
        public int Id { get; set; }

        /// <summary>KT-12 öncesi denemelerde boş.</summary>
        public Guid? OrderNumber { get; set; }

        public string UserId { get; set; }

        public int OrderingId { get; set; }

        public bool IsSuccess { get; set; }

        /// <summary>Başarısız denemede bankanın mesajı.</summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedDate { get; set; }

        /// <summary>Maskeli kart numarası: <c>**** 1234</c>.</summary>
        [MaxLength(19)]
        public string CardNumber { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }
    }
}
