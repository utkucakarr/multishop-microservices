namespace MultiShop.DtoLayer.PaymentDto
{
    /// <summary>Ödeme isteği: sipariş numarası ve kart. Tutar ve kullanıcı gönderilmez (Payment siparişten ve token'dan alır).</summary>
    public class CreatePaymentDto
    {
        public Guid OrderNumber { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerName { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }

        public string Cvv { get; set; }
    }
}
