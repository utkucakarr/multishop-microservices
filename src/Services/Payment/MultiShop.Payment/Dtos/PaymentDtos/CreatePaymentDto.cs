namespace MultiShop.Payment.Dtos.PaymentDtos
{
    public class CreatePaymentDto
    {
        public string UserId { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerName { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }

        public string Cvv { get; set; }

        public string PaymentAmounth { get; set; }

        public int OrderingId { get; set; }
    }
}
