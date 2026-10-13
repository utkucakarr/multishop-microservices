using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Dispatching;
using MultiShop.Notification.Dtos;

namespace MultiShop.Notification.Services;

public class NotificationInboxService : INotificationInboxService
{
    private const int MaxPageSize = 100;

    private readonly NotificationDbContext _db;
    private readonly DeliverySignal _deliverySignal;

    public NotificationInboxService(NotificationDbContext db, DeliverySignal deliverySignal)
    {
        _db = db;
        _deliverySignal = deliverySignal;
    }

    // Admin kutusu yalnızca admin bildirimlerini gösterir; kullanıcıya özel bildirimler (KT-13) ayrı listelenir.
    private IQueryable<NotificationRecord> AdminNotifications
        => _db.Notifications.AsNoTracking().Where(x => x.RecipientType == NotificationRecipientType.Admins);

    public async Task<PagedResultDto<NotificationListItemDto>> GetListAsync(int page, int pageSize, string? type, bool unreadOnly, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = AdminNotifications;
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.Type == type);
        if (unreadOnly)
            query = query.Where(x => x.ReadAt == null);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationListItemDto(
                x.Id, x.Type, x.Title, x.Message, x.Link, x.CreatedAt, x.ReadAt, x.ReadBy,
                x.Deliveries.Count,
                x.Deliveries.Count(d => d.Status == DeliveryStatus.Failed)))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<NotificationListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<NotificationDetailDto> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await AdminNotifications.Include(x => x.Deliveries).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Bildirim", id);

        var deliveries = notification.Deliveries
            .OrderBy(x => x.Channel).ThenBy(x => x.CreatedAt)
            .Select(x => new DeliveryDto(
                x.Id, x.Channel, RecipientMask.Mask(x.Channel, x.Recipient), x.Status, x.Attempts,
                x.Status == DeliveryStatus.Pending ? x.NextAttemptAt : null,
                x.LastError, x.SentAt, x.Status == DeliveryStatus.Failed))
            .ToList();

        return new NotificationDetailDto(notification.Id, notification.Type, notification.Title, notification.Message, notification.Link,
            notification.CreatedAt, notification.ReadAt, notification.ReadBy, deliveries);
    }

    public Task<int> GetUnreadCountAsync(CancellationToken cancellationToken)
        => AdminNotifications.CountAsync(x => x.ReadAt == null, cancellationToken);

    public async Task MarkAsReadAsync(Guid id, string readBy, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.RecipientType == NotificationRecipientType.Admins, cancellationToken)
            ?? throw new NotFoundException("Bildirim", id);

        // Okundu bilgisi ortak: ilk okuyan admin kaydedilir.
        if (notification.ReadAt is not null)
            return;

        notification.ReadAt = DateTimeOffset.UtcNow;
        notification.ReadBy = readBy;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> MarkAllAsReadAsync(string readBy, CancellationToken cancellationToken)
        => _db.Notifications
            .Where(x => x.RecipientType == NotificationRecipientType.Admins && x.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReadAt, DateTimeOffset.UtcNow).SetProperty(x => x.ReadBy, readBy), cancellationToken);

    public async Task RetryDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await _db.Deliveries.FirstOrDefaultAsync(x => x.Id == deliveryId, cancellationToken)
            ?? throw new NotFoundException("Gönderim", deliveryId);
        if (delivery.Status != DeliveryStatus.Failed)
            throw new BadRequestException("Yalnızca başarısız gönderimler tekrar gönderilebilir.");

        // Elle tekrar gönderme yeni bir deneme döngüsü başlatır (RetryPolicy baştan uygulanır).
        delivery.Status = DeliveryStatus.Pending;
        delivery.Attempts = 0;
        delivery.NextAttemptAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _deliverySignal.Notify();
    }
}
