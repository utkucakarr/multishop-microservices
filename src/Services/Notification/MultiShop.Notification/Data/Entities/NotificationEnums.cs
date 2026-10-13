namespace MultiShop.Notification.Data.Entities;

// Veritabanında metin olarak saklanır (ör. "Email"); sayısal değerler yalnızca koddaki sıralama içindir.

public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
    Sms = 3,
}

public enum NotificationRecipientType
{
    Admins = 1,
    User = 2,
}

/// <summary>Gönderim outbox'ındaki bir satırın durumu (yalnızca Email/Sms; InApp için satır tutulmaz).</summary>
public enum DeliveryStatus
{
    /// <summary>Gönderilmeyi bekliyor (yeni ya da tekrar deneme zamanı gelmiş).</summary>
    Pending = 1,
    /// <summary>İşlemci sahiplendi, gönderiliyor. Uzun süre burada kalırsa (çökme) tekrar Pending yapılır.</summary>
    Processing = 2,
    Sent = 3,
    /// <summary>Tüm denemeler başarısız; panelden elle tekrar gönderilebilir.</summary>
    Failed = 4,
    /// <summary>Kanalın gerçek sağlayıcısı yok (ör. SMS); gönderilmedi, yalnızca loglandı.</summary>
    Simulated = 5,
}
