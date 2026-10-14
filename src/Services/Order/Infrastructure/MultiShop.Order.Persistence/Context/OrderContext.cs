using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Order.Domain.Entities;
using MultiShop.Order.Persistence.Sagas;

namespace MultiShop.Order.Persistence.Context
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Ordering> Orderings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ordering>().HasIndex(x => x.OrderNumber).IsUnique();

            // Sipariş saga'sının kaydı (OrderStates). Saga, sipariş ve outbox aynı DbContext'i kullanır:
            // bir mesajın tüm etkisi (sipariş satırı, saga durumu, gönderilecek mesajlar) tek transaction'da yazılır.
            new OrderStateMap().Configure(modelBuilder);

            // MassTransit tabloları: InboxState (aynı mesaj ikinci kez işlenmez), OutboxMessage/OutboxState
            // (saga'nın gönderdiği komut ve event'ler commit'ten sonra RabbitMQ'ya iletilir).
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
