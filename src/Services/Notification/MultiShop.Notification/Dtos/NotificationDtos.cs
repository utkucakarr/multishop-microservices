using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Dtos;

// Enum'lar JSON'da metin olarak gider (Program.cs: JsonStringEnumConverter), ör. "Email", "Failed".

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed record NotificationListItemDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Link,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    string? ReadBy,
    int DeliveryCount,
    int FailedDeliveryCount);

public sealed record NotificationDetailDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Link,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    string? ReadBy,
    IReadOnlyList<DeliveryDto> Deliveries);

/// <param name="Recipient">Maskeli (ör. <c>u***@ornek.com</c>, <c>*******1234</c>).</param>
/// <param name="CanRetry">Yalnızca kalıcı olarak başarısız (Failed) gönderim panelden tekrar gönderilebilir.</param>
public sealed record DeliveryDto(
    Guid Id,
    NotificationChannel Channel,
    string Recipient,
    DeliveryStatus Status,
    int Attempts,
    DateTimeOffset? NextAttemptAt,
    string? LastError,
    DateTimeOffset? SentAt,
    bool CanRetry);
