using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Entities;

namespace MultiShop.Comment.Context
{
    public class CommentContext : DbContext
    {
        public CommentContext(DbContextOptions<CommentContext> options) : base(options)
        {
            
        }

        public DbSet<UserComment> UserComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserComment>(entity =>
            {
                entity.Property(x => x.UserId).HasMaxLength(CommentLimits.UserIdMaxLength);
                entity.Property(x => x.NameSurname).HasMaxLength(CommentLimits.NameSurnameMaxLength);
                entity.Property(x => x.Email).HasMaxLength(CommentLimits.EmailMaxLength);
                entity.Property(x => x.CommentDetail).HasMaxLength(CommentLimits.DetailMaxLength);
                entity.Property(x => x.ProductId).HasMaxLength(CommentLimits.ProductIdMaxLength);
                // Ürün sayfası: bir ürünün onaylı yorumları, en yeni önce.
                entity.HasIndex(x => new { x.ProductId, x.Status });
            });

            // MassTransit outbox tabloları: gönderilmeyi bekleyen event'ler (OutboxMessage, OutboxState).
            // InboxState de eklenir; MassTransit'in temizlik servisi bu tabloyu bekler.
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
