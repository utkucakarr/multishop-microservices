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

            // MassTransit outbox tabloları: gönderilmeyi bekleyen event'ler (OutboxMessage, OutboxState).
            // InboxState de eklenir; MassTransit'in temizlik servisi bu tabloyu bekler.
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
