namespace MultiShop.Notification.Notifications;

/// <param name="Type">İstemcinin simge/renk seçmesi için tür, ör. <c>comment</c>, <c>contact-message</c>, <c>test</c>.</param>
/// <param name="Link">Tıklanınca gidilecek admin sayfası (isteğe bağlı), ör. <c>/Admin/Comment/Index</c>.</param>
public sealed record NotificationMessage(string Type, string Title, string Message, string? Link, DateTimeOffset CreatedAt);
