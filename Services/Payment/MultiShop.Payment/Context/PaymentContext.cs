using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Entities;

namespace MultiShop.Payment.Context
{
    public class PaymentContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost,1440;initial Catalog=MultiShopPaymentDb;User=sa;Password=123456aA*");
        }

        public DbSet<PaymentInfo> PaymentInfos { get; set; }
    }
}
