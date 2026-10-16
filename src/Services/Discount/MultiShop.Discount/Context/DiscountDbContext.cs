using Microsoft.EntityFrameworkCore;
using MultiShop.Discount.Entities;

namespace MultiShop.Discount.Context
{
    /// <summary>
    /// Yalnızca şema (migration) ve health check için EF Core. Sorgular Dapper ile <see cref="IDbConnectionFactory"/> üzerinden.
    /// Önceki DapperContext ikisini tek sınıfta birleştiriyordu ve transient kayıtlıydı (DIS-04).
    /// </summary>
    public class DiscountDbContext : DbContext
    {
        public DiscountDbContext(DbContextOptions<DiscountDbContext> options) : base(options)
        {
        }

        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>(entity =>
            {
                // Kod müşterinin yazdığı anahtar: iki kupon aynı kodu taşırsa hangisinin uygulanacağı belirsiz olurdu (DIS-02).
                entity.Property(x => x.Code).HasMaxLength(CouponLimits.CodeMaxLength);
                entity.HasIndex(x => x.Code).IsUnique();
            });
        }
    }
}
