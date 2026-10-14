using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Context
{
    public class PaymentContext : DbContext
    {
        public PaymentContext(DbContextOptions<PaymentContext> options) : base (options)
        {
            
        }

        public DbSet<PaymentInfo> PaymentInfos { get; set; }

        public DbSet<OrderPayment> OrderPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderPayment>().HasIndex(x => x.OrderNumber).IsUnique();
            modelBuilder.Entity<OrderPayment>().Property(x => x.UserId).HasMaxLength(450);
            modelBuilder.Entity<PaymentInfo>().HasIndex(x => x.OrderNumber);

            // MassTransit tabloları: InboxState (aynı event ikinci kez işlenmez), OutboxMessage/OutboxState
            // (PaymentSucceeded/PaymentFailed ödeme sonucuyla aynı transaction'da yazılır, sonra RabbitMQ'ya gider).
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
