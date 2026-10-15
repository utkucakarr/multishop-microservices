namespace MultiShop.DtoLayer.PaymentDto
{
    public class CreatePaymentResponseDto
    {
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>Denemeden sonraki ödeme durumu: Succeeded, AwaitingPayment, Failed, Cancelled.</summary>
        public string? Status { get; set; }

        /// <summary>Kalan deneme hakkı (ödeme beklemeye devam ediyorsa).</summary>
        public int RemainingAttempts { get; set; }
    }
}
