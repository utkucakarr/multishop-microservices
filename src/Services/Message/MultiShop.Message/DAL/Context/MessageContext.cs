using Microsoft.EntityFrameworkCore;
using MultiShop.Message.DAL.Entities;

namespace MultiShop.Message.DAL.Context
{
    public class MessageContext : DbContext
    {
        public MessageContext(DbContextOptions<MessageContext> options) : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.Property(x => x.NameSurname).HasMaxLength(ContactMessageLimits.NameSurname);
                entity.Property(x => x.Email).HasMaxLength(ContactMessageLimits.Email);
                entity.Property(x => x.Subject).HasMaxLength(ContactMessageLimits.Subject);
                entity.Property(x => x.Message).HasMaxLength(ContactMessageLimits.Message);
                // Admin listesi ve okunmamış sayacı bu iki alana göre sorgular.
                entity.HasIndex(x => x.SendDate);
                entity.HasIndex(x => x.IsRead);
            });
        }
    }
}
