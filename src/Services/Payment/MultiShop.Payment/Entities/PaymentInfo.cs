namespace MultiShop.Payment.Entities
{
    public class PaymentInfo
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int OrderingId { get; set; }

        public string BankResponse { get; set; }

        public bool IsSuccess { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CardNumber { get; set; }

        public ulong Amount { get; set; }
    }
}
