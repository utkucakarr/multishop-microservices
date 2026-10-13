using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Channels;

/// <summary>
/// Strategy: her kanal (panel içi, mail, SMS) aynı arayüzü uygular; Dispatcher ve DeliveryProcessor
/// kanalı bilmeden çağırır. Yeni kanal = yeni sınıf + DI kaydı (Open/Closed).
/// </summary>
public interface INotificationChannel
{
    NotificationChannel Channel { get; }

    /// <summary>Kanalın garanti düzeyi; Dispatcher buna göre davranır (kararlar N-3, N-4).</summary>
    DeliveryMode Mode { get; }

    /// <summary>Başarısızlıkta exception fırlatır; Outbox kanallarında DeliveryProcessor tekrar dener.</summary>
    Task<DeliveryResult> SendAsync(ChannelMessage message, CancellationToken cancellationToken);
}

public enum DeliveryMode
{
    /// <summary>State + ephemeral event: kayıt kalıcı, gönderim commit'ten sonra ve best effort (InApp).</summary>
    Immediate,
    /// <summary>Transactional outbox: gönderim satırı bildirimle aynı transaction'da, işlemci retry ile gönderir (Email, Sms).</summary>
    Outbox,
}

public enum DeliveryResult
{
    Sent,
    /// <summary>Gerçek sağlayıcı yok; gönderilmedi, yalnızca loglandı.</summary>
    Simulated,
}

/// <param name="Recipient">Outbox kanallarında mail adresi / telefon; InApp'te null.</param>
/// <param name="Subject">Yalnızca mail.</param>
public sealed record ChannelMessage(NotificationRecord Notification, string? Recipient, string? Subject, string Body);
