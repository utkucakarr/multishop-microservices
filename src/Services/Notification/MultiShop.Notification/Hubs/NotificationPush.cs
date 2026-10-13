namespace MultiShop.Notification.Hubs;

/// <summary>SignalR ile istemciye giden bildirim (JavaScript'te camelCase: id, type, title, message, link, createdAt).</summary>
public sealed record NotificationPush(Guid Id, string Type, string Title, string Message, string? Link, DateTimeOffset CreatedAt);
