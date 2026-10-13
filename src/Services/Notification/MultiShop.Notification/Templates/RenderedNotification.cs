namespace MultiShop.Notification.Templates;

/// <summary>Bir şablonun ürettiği metinler: panel içi bildirim + kanal başına içerik.</summary>
public sealed record RenderedNotification(
    string Title,
    string Message,
    string? Link,
    string EmailSubject,
    string EmailBody,
    string SmsText);
