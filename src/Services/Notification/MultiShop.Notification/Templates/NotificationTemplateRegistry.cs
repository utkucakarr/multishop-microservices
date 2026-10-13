namespace MultiShop.Notification.Templates;

public class NotificationTemplateRegistry
{
    private readonly Dictionary<string, INotificationTemplate> _templates;

    public NotificationTemplateRegistry(IEnumerable<INotificationTemplate> templates)
    {
        _templates = templates.ToDictionary(x => x.Type, StringComparer.Ordinal);
    }

    public INotificationTemplate Get(string type)
        => _templates.TryGetValue(type, out var template)
            ? template
            : throw new InvalidOperationException($"'{type}' bildirim türü için şablon yok.");
}
