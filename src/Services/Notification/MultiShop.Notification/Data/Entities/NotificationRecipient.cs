namespace MultiShop.Notification.Data.Entities;

/// <summary>
/// Admin bildirimlerinin gideceği mail adresi ya da telefon numarası. Panelden eklenir; repoya kişisel veri
/// girmesin diye seed yoktur. Kullanıcı tablosu (Identity) kullanılmaz: servis sınırı ve farklı adres (ör. destek@) ihtiyacı.
/// </summary>
public class NotificationRecipient
{
    public Guid Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Address { get; set; } = default!;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
