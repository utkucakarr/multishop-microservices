namespace MultiShop.Notification.Data.Entities;

/// <summary>
/// Bir bildirim: admin panelindeki bildirim kutusunun satırı. Panel içi (InApp) bildirimin kendisi budur
/// (state + ephemeral event: kayıt kalıcı, SignalR push yalnızca ipucu).
/// </summary>
/// <remarks>Adı <c>Notification</c> değil: proje ad alanı <c>MultiShop.Notification</c> ile çakışır (CS0118).</remarks>
public class NotificationRecord
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Link { get; set; }
    public NotificationRecipientType RecipientType { get; set; }
    /// <summary>Kullanıcıya özel bildirimlerde (KT-13) alıcının <c>sub</c> değeri.</summary>
    public string? RecipientUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Okundu bilgisi ortak: bir admin okuyunca herkes için okunmuş sayılır.</summary>
    public DateTimeOffset? ReadAt { get; set; }
    public string? ReadBy { get; set; }

    public List<NotificationDelivery> Deliveries { get; set; } = new();
}
