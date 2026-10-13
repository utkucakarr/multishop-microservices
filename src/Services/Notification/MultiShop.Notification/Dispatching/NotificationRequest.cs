using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Dispatching;

/// <summary>
/// "Ne oldu, kime": tüketici (event consumer, test ucu) yalnızca bunu söyler; hangi kanaldan gideceğine
/// Dispatcher kurallara bakarak karar verir.
/// </summary>
/// <param name="Type">Bildirim türü, ör. <see cref="Data.NotificationTypes.ContactMessageReceived"/>.</param>
/// <param name="Data">Şablonun kullandığı alanlar (türe özgü).</param>
/// <param name="Channels">Boş değilse kural yerine yalnızca bu kanallar kullanılır (panelde kanal başına test).</param>
public sealed record NotificationRequest(
    string Type,
    NotificationTarget Target,
    IReadOnlyDictionary<string, string> Data,
    IReadOnlyCollection<NotificationChannel>? Channels = null);

/// <summary>Alıcı: admin'ler (adresler Bildirim Ayarları'ndaki alıcı listesinden) ya da tek kullanıcı (KT-13).</summary>
public sealed record NotificationTarget(NotificationRecipientType Type, string? UserId = null, string? Email = null, string? Phone = null)
{
    public static NotificationTarget Admins { get; } = new(NotificationRecipientType.Admins);
}
