namespace Messages.Events
{
    // Order API siparişi oluşturduğunda RabbitMQ'ya bu sınıfı gönderecek.
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public PaymentMessage Payment { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
