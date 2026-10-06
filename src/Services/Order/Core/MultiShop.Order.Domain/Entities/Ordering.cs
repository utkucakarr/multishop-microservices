using MultiShop.Order.Domain.Enums;

namespace MultiShop.Order.Domain.Entities
{
    public class Ordering
    {
        public int OrderingId { get; set; }

        public string UserId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        // Siparişin RabbitMQ üzerindeki serüvenini takip edeceğimiz durum alanı
        public OrderStatus Status { get; set; }

        // Eğer ödeme başarısız olursa veya stok biterse, neden iptal edildiğini tutacağımız alan
        public string? FailMessage { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
