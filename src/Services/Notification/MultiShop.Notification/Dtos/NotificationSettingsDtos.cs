using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Dtos;

/// <param name="Warnings">Ör. "Mail kanalı açık ama aktif mail alıcısı yok"; panelde sarı uyarı olarak gösterilir.</param>
public sealed record NotificationSettingsDto(
    IReadOnlyList<NotificationRuleDto> Rules,
    IReadOnlyList<NotificationRecipientDto> Recipients,
    IReadOnlyList<string> Warnings);

public sealed record NotificationRuleDto(string Type, string DisplayName, bool InAppEnabled, bool EmailEnabled, bool SmsEnabled);

public sealed record UpdateNotificationRuleDto(string Type, bool InAppEnabled, bool EmailEnabled, bool SmsEnabled);

public sealed record NotificationRecipientDto(Guid Id, NotificationChannel Channel, string Address, string? Name, bool IsActive);

public sealed record CreateNotificationRecipientDto(NotificationChannel Channel, string Address, string? Name);

public sealed record UpdateNotificationRecipientDto(bool IsActive);
