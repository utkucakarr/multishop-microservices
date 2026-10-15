using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();
    public DbSet<NotificationDelivery> Deliveries => Set<NotificationDelivery>();
    public DbSet<NotificationRule> Rules => Set<NotificationRule>();
    public DbSet<NotificationRecipient> Recipients => Set<NotificationRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MassTransit inbox: işlenen event'lerin kaydı (InboxState). Outbox tabloları da gerekir; consumer'ın
        // yayınlayacağı event'ler (ileride) aynı transaction'da OutboxMessage'a yazılır.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(x => x.Type).HasMaxLength(NotificationLimits.Type);
            entity.Property(x => x.Title).HasMaxLength(NotificationLimits.Title);
            entity.Property(x => x.Message).HasMaxLength(NotificationLimits.Message);
            entity.Property(x => x.Link).HasMaxLength(NotificationLimits.Link);
            entity.Property(x => x.RecipientType).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.RecipientUserId).HasMaxLength(NotificationLimits.UserId);
            entity.Property(x => x.ReadBy).HasMaxLength(NotificationLimits.UserName);
            // Liste en yeniden eskiye; zil okunmamışları sayar.
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.ReadAt);
        });

        modelBuilder.Entity<NotificationDelivery>(entity =>
        {
            entity.ToTable("NotificationDeliveries");
            entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Recipient).HasMaxLength(NotificationLimits.Address);
            entity.Property(x => x.Subject).HasMaxLength(NotificationLimits.Subject);
            entity.Property(x => x.Body).HasMaxLength(NotificationLimits.Body);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.LastError).HasMaxLength(NotificationLimits.Error);
            entity.HasOne(x => x.Notification).WithMany(x => x.Deliveries).HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
            // İşlemcinin sorgusu: Status = Pending AND NextAttemptAt <= şimdi.
            entity.HasIndex(x => new { x.Status, x.NextAttemptAt });
        });

        modelBuilder.Entity<NotificationRule>(entity =>
        {
            entity.ToTable("NotificationRules");
            entity.HasKey(x => x.Type);
            entity.Property(x => x.Type).HasMaxLength(NotificationLimits.Type);
            entity.Property(x => x.DisplayName).HasMaxLength(NotificationLimits.DisplayName);
            // Varsayılanlar; admin panelden değiştirir.
            entity.HasData(
                new NotificationRule { Type = NotificationTypes.ContactMessageReceived, DisplayName = "İletişim formu mesajı", InAppEnabled = true, EmailEnabled = true, SmsEnabled = false },
                new NotificationRule { Type = NotificationTypes.CommentCreated, DisplayName = "Yeni yorum", InAppEnabled = true, EmailEnabled = false, SmsEnabled = false },
                new NotificationRule { Type = NotificationTypes.Test, DisplayName = "Test bildirimi", InAppEnabled = true, EmailEnabled = true, SmsEnabled = true },
                // KT-13. Müşterinin zili yok (kullanıcı panelinde SignalR yok): müşteri türleri yalnızca e-posta.
                new NotificationRule { Type = NotificationTypes.OrderPaid, DisplayName = "Sipariş onayı (müşteriye)", InAppEnabled = false, EmailEnabled = true, SmsEnabled = false },
                new NotificationRule { Type = NotificationTypes.NewOrder, DisplayName = "Yeni sipariş (admin'e)", InAppEnabled = true, EmailEnabled = false, SmsEnabled = false },
                new NotificationRule { Type = NotificationTypes.OrderShipped, DisplayName = "Kargoya verildi (müşteriye)", InAppEnabled = false, EmailEnabled = true, SmsEnabled = false });
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.ToTable("NotificationRecipients");
            entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Address).HasMaxLength(NotificationLimits.Address);
            entity.Property(x => x.Name).HasMaxLength(NotificationLimits.RecipientName);
            // Aynı adres aynı kanala iki kez eklenmesin.
            entity.HasIndex(x => new { x.Channel, x.Address }).IsUnique();
        });
    }
}
