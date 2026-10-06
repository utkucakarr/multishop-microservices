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
    }
}
