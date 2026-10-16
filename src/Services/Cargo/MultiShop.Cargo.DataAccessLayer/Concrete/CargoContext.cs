using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Cargo.EntityLayer.Concrete;

namespace MultiShop.Cargo.DataAccessLayer.Concrete
{
    public class CargoContext : DbContext
    {
        public CargoContext(DbContextOptions<CargoContext> options) : base(options)
        {
            
        }

        public DbSet<CargoCompany> CargoCompanies { get; set; }

        /// <summary>Gönderiler (KT-13: siparişe bağlı).</summary>
        public DbSet<CargoDetail> CargoDetails { get; set; }

        /// <summary>Gönderilerin hareket geçmişi.</summary>
        public DbSet<CargoOperation> CargoOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CargoCompany>().Property(x => x.CargoCompanyName).HasMaxLength(CargoCompany.NameMaxLength);

            modelBuilder.Entity<CargoDetail>(entity =>
            {
                // Bir siparişin tek gönderisi: aynı komut iki kez gelse ikinci gönderi açılmaz (idempotent consumer).
                entity.HasIndex(x => x.OrderNumber).IsUnique();
                entity.HasIndex(x => x.Barcode).IsUnique();
                entity.Property(x => x.Barcode).HasMaxLength(32);
                entity.Property(x => x.UserId).HasMaxLength(450);
                entity.Property(x => x.ReceiverName).HasMaxLength(200);
                entity.Property(x => x.ReceiverEmail).HasMaxLength(256);
                entity.Property(x => x.ReceiverPhone).HasMaxLength(50);
                entity.HasOne(x => x.CargoCompany).WithMany().HasForeignKey(x => x.CargoCompanyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.Operations).WithOne(x => x.CargoDetail).HasForeignKey(x => x.CargoDetailId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CargoOperation>().Property(x => x.Barcode).HasMaxLength(32);

            // MassTransit tabloları: InboxState (aynı komut ikinci kez işlenmez), OutboxMessage/OutboxState
            // (ShipmentDispatched/ShipmentDelivered gönderi durumuyla aynı transaction'da yazılır).
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
