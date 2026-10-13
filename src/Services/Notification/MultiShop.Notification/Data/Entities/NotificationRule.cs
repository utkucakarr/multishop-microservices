namespace MultiShop.Notification.Data.Entities;

/// <summary>Bir bildirim türünün hangi kanallardan gideceği; admin panelindeki "Bildirim Ayarları"ndan değiştirilir.</summary>
public class NotificationRule
{
    /// <summary>Bildirim türü, ör. <c>contact-message-received</c>. Şablonlar da bu anahtarla bulunur.</summary>
    public string Type { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public bool InAppEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
