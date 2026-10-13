using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MultiShop.Discount.Entities;
using System.Data;

namespace MultiShop.Discount.Context
{
    public class DapperContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
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

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
