namespace MultiShop.Notification.Templates;

/// <summary>
/// Bir bildirim türünün metinleri. Şablonlar şimdilik kodda (karar N-7); panelden düzenleme sonra.
/// <paramref name="data"/> anahtarları türe özgüdür (ör. iletişim mesajında <c>Name</c>, <c>Subject</c>).
/// </summary>
public interface INotificationTemplate
{
    string Type { get; }
    RenderedNotification Render(IReadOnlyDictionary<string, string> data);
}

internal static class TemplateData
{
    /// <summary>Eksik alan bildirimi düşürmesin; metinde "-" görünür.</summary>
    public static string Get(this IReadOnlyDictionary<string, string> data, string key)
        => data.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : "-";
}
