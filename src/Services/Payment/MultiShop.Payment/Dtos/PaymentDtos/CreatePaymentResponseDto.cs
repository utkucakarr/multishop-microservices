using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Dtos.PaymentDtos
{
    public class CreatePaymentResponseDto
    {
        public bool IsSuccess { get; set; }

        /// <summary>Başarısız denemede bankanın mesajı.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Denemeden sonraki durum: Succeeded, AwaitingPayment (tekrar denenebilir), Failed (hak bitti), Cancelled.</summary>
        public OrderPaymentStatus Status { get; set; }

        public int RemainingAttempts { get; set; }
    }
}
