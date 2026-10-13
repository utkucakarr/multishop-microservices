using Microsoft.EntityFrameworkCore;
using MultiShop.Notification.Channels;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Templates;

namespace MultiShop.Notification.Dispatching;

/// <summary>
/// 1) Kuralı oku  2) şablonu uygula  3) TEK TRANSACTION: bildirim + Outbox kanallarının gönderim satırları
/// 4) commit'ten sonra (<see cref="PostCommitActions"/>): Immediate kanalları (InApp, best effort) çalıştır, DeliveryProcessor'a sinyal ver.
/// Dispatcher hiçbir Outbox kanalını kendisi çağırmaz; gönderimi DeliveryProcessor yapar.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly NotificationDbContext _db;
    private readonly NotificationTemplateRegistry _templates;
    private readonly NotificationChannelRegistry _channels;
    private readonly DeliverySignal _deliverySignal;
    private readonly PostCommitActions _postCommitActions;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        NotificationDbContext db,
        NotificationTemplateRegistry templates,
        NotificationChannelRegistry channels,
        DeliverySignal deliverySignal,
        PostCommitActions postCommitActions,
        ILogger<NotificationDispatcher> logger)
    {
        _db = db;
        _templates = templates;
        _channels = channels;
        _deliverySignal = deliverySignal;
        _postCommitActions = postCommitActions;
        _logger = logger;
    }

    public async Task<Guid?> DispatchAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var enabledChannels = request.Channels is { Count: > 0 }
            ? request.Channels.Distinct().ToList()
            : await GetChannelsFromRuleAsync(request.Type, cancellationToken);
        if (enabledChannels.Count == 0)
        {
            _logger.LogInformation("{NotificationType} için açık kanal yok; bildirim oluşturulmadı.", request.Type);
            return null;
        }

        var content = _templates.Get(request.Type).Render(request.Data);
        var now = DateTimeOffset.UtcNow;
        var inAppEnabled = enabledChannels.Contains(NotificationChannel.InApp);

        var notification = new NotificationRecord
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Title = content.Title,
            Message = content.Message,
            Link = content.Link,
            RecipientType = request.Target.Type,
            RecipientUserId = request.Target.UserId,
            CreatedAt = now,
            // Panel içi kapalıysa kayıt yine tutulur (gönderim geçmişi panelde görünsün) ama zile düşmez.
            ReadAt = inAppEnabled ? null : now,
            ReadBy = inAppEnabled ? null : "system",
        };

        foreach (var channel in enabledChannels.Where(x => _channels.Get(x).Mode == DeliveryMode.Outbox))
        {
            var recipients = await GetRecipientsAsync(channel, request.Target, cancellationToken);
            if (recipients.Count == 0)
            {
                _logger.LogWarning("{NotificationType}: {Channel} kanalı açık ama aktif alıcı yok; gönderim oluşturulmadı.", request.Type, channel);
                continue;
            }

            foreach (var recipient in recipients)
            {
                notification.Deliveries.Add(new NotificationDelivery
                {
                    Id = Guid.NewGuid(),
                    Channel = channel,
                    Recipient = recipient,
                    Subject = channel == NotificationChannel.Email ? content.EmailSubject : null,
                    Body = channel == NotificationChannel.Email ? content.EmailBody : content.SmsText,
                    Status = DeliveryStatus.Pending,
                    NextAttemptAt = now,
                    CreatedAt = now,
                });
            }
        }

        // Commit'ten SONRA çalışır: önce gönderilirse ve transaction geri alınırsa "hayalet bildirim" olurdu.
        // SaveChanges'ten önce eklenir: transaction yoksa (test ucu) SaveChanges'in kendisi commit'tir.
        _postCommitActions.Add(token => AfterCommitAsync(notification, enabledChannels, token));

        // Transactional outbox: bildirim ve gönderim satırları tek SaveChanges = tek transaction.
        // Event consumer'da bu SaveChanges MassTransit inbox'ının transaction'ı içinde kalır; commit consumer bitince olur.
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bildirim oluşturuldu: {NotificationType} {NotificationId}, {DeliveryCount} gönderim kuyrukta.",
            request.Type, notification.Id, notification.Deliveries.Count);
        return notification.Id;
    }

    private async Task AfterCommitAsync(NotificationRecord notification, List<NotificationChannel> enabledChannels, CancellationToken cancellationToken)
    {
        if (notification.Deliveries.Count > 0)
            _deliverySignal.Notify();

        foreach (var channel in enabledChannels.Where(x => _channels.Get(x).Mode == DeliveryMode.Immediate))
        {
            try
            {
                await _channels.Get(channel).SendAsync(new ChannelMessage(notification, null, null, notification.Message), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Ephemeral event: kayıt zaten kalıcı, istemci yeniden bağlanınca durumu okur.
                _logger.LogWarning(ex, "{NotificationType}: {Channel} anlık gönderimi başarısız (kayıt kalıcı).", notification.Type, channel);
            }
        }
    }

    private async Task<List<NotificationChannel>> GetChannelsFromRuleAsync(string type, CancellationToken cancellationToken)
    {
        var rule = await _db.Rules.AsNoTracking().FirstOrDefaultAsync(x => x.Type == type, cancellationToken)
            ?? throw new InvalidOperationException($"'{type}' bildirim türü için kural yok.");

        var channels = new List<NotificationChannel>();
        if (rule.InAppEnabled) channels.Add(NotificationChannel.InApp);
        if (rule.EmailEnabled) channels.Add(NotificationChannel.Email);
        if (rule.SmsEnabled) channels.Add(NotificationChannel.Sms);
        return channels;
    }

    private async Task<List<string>> GetRecipientsAsync(NotificationChannel channel, NotificationTarget target, CancellationToken cancellationToken)
    {
        if (target.Type == NotificationRecipientType.User)
        {
            var address = channel == NotificationChannel.Email ? target.Email : target.Phone;
            return string.IsNullOrWhiteSpace(address) ? new List<string>() : new List<string> { address };
        }

        return await _db.Recipients.AsNoTracking()
            .Where(x => x.Channel == channel && x.IsActive)
            .Select(x => x.Address)
            .ToListAsync(cancellationToken);
    }
}
