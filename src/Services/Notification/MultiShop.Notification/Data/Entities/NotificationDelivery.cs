namespace MultiShop.Notification.Data.Entities;

/// <summary>
/// Gönderim outbox'ının satırı: bir bildirimin bir alıcıya bir kanaldan (Email/Sms) gönderimi.
/// Bildirimle aynı transaction'da yazılır; DeliveryProcessor gönderir ve sonucu işler.
/// Gönderilecek içerik (konu/gövde) satırın içindedir: işlemci şablonu ya da event verisini yeniden aramaz.
/// </summary>
public class NotificationDelivery
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public NotificationRecord Notification { get; set; } = default!;

    public NotificationChannel Channel { get; set; }
    /// <summary>Mail adresi ya da telefon numarası.</summary>
    public string Recipient { get; set; } = default!;
    /// <summary>Yalnızca Email.</summary>
    public string? Subject { get; set; }
    public string Body { get; set; } = default!;

    public DeliveryStatus Status { get; set; }
    public int Attempts { get; set; }
    /// <summary>Pending satırın en erken ne zaman gönderileceği (tekrar denemelerde ileri atılır).</summary>
    public DateTimeOffset NextAttemptAt { get; set; }
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
