namespace Messages.Events
{
    public class OrderItemMessage
    {
        public string ProductId { get; set; } // Catalog servisinin hangi stoku düşeceğini bilmesi için
        public int Count { get; set; }
    }
}
