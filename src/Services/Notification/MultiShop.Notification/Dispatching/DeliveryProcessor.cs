using Microsoft.EntityFrameworkCore;
using MultiShop.Notification.Channels;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Dispatching;

/// <summary>
/// Gönderim outbox'ının işlemcisi. Sırası gelen Pending satırları sahiplenir (Pending → Processing), ilgili kanal
/// stratejisiyle gönderir, sonucu yazar. Hata olursa RetryPolicy'ye göre tekrar dener (at-least-once).
/// Uyanma: DeliverySignal (hız) ya da en geç <see cref="SweepInterval"/> (garanti).
/// </summary>
public class DeliveryProcessor : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(30);
    // Bu süreden uzun Processing'de kalan satır, işlemcinin gönderim sırasında çöktüğünü gösterir.
    private static readonly TimeSpan StuckAfter = TimeSpan.FromMinutes(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationChannelRegistry _channels;
    private readonly DeliverySignal _signal;
    private readonly ILogger<DeliveryProcessor> _logger;

    public DeliveryProcessor(IServiceScopeFactory scopeFactory, NotificationChannelRegistry channels, DeliverySignal signal, ILogger<DeliveryProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _channels = channels;
        _signal = signal;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueDeliveriesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Ör. veritabanına geçici olarak ulaşılamıyor; bir sonraki turda tekrar denenir.
                _logger.LogError(ex, "Gönderim outbox'ı işlenirken hata.");
            }

            try
            {
                await _signal.WaitAsync(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessDueDeliveriesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var stuckBefore = DateTimeOffset.UtcNow - StuckAfter;
        var recovered = await db.Deliveries
            .Where(x => x.Status == DeliveryStatus.Processing && x.ProcessingStartedAt < stuckBefore)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, DeliveryStatus.Pending).SetProperty(x => x.ProcessingStartedAt, (DateTimeOffset?)null), cancellationToken);
        if (recovered > 0)
            _logger.LogWarning("{Count} gönderim Processing'de takılı kalmıştı; tekrar kuyruğa alındı.", recovered);

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var dueIds = await db.Deliveries
                .Where(x => x.Status == DeliveryStatus.Pending && x.NextAttemptAt <= now)
                .OrderBy(x => x.NextAttemptAt)
                .Select(x => x.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            if (dueIds.Count == 0)
                return;

            foreach (var id in dueIds)
            {
                // Sahiplenme: yalnızca hâlâ Pending ise Processing yap. 0 satır = başka bir döngü/örnek aldı.
                // Birden fazla örnekte ölçeklenirse PostgreSQL "FOR UPDATE SKIP LOCKED" ile değiştirilebilir.
                var claimed = await db.Deliveries
                    .Where(x => x.Id == id && x.Status == DeliveryStatus.Pending)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, DeliveryStatus.Processing).SetProperty(x => x.ProcessingStartedAt, DateTimeOffset.UtcNow), cancellationToken);
                if (claimed == 0)
                    continue;

                var delivery = await db.Deliveries.Include(x => x.Notification).FirstAsync(x => x.Id == id, cancellationToken);
                await SendAsync(db, delivery, cancellationToken);
            }

            db.ChangeTracker.Clear();
        }
    }

    private async Task SendAsync(NotificationDbContext db, NotificationDelivery delivery, CancellationToken cancellationToken)
    {
        delivery.Attempts++;
        try
        {
            var message = new ChannelMessage(delivery.Notification, delivery.Recipient, delivery.Subject, delivery.Body);
            var result = await _channels.Get(delivery.Channel).SendAsync(message, cancellationToken);

            delivery.Status = result == DeliveryResult.Simulated ? DeliveryStatus.Simulated : DeliveryStatus.Sent;
            delivery.SentAt = DateTimeOffset.UtcNow;
            delivery.LastError = null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            delivery.LastError = ex.Message.Length > NotificationLimits.Error ? ex.Message[..NotificationLimits.Error] : ex.Message;
            var retryDelay = RetryPolicy.GetRetryDelay(delivery.Attempts);
            if (retryDelay is { } delay)
            {
                delivery.Status = DeliveryStatus.Pending;
                delivery.NextAttemptAt = DateTimeOffset.UtcNow + delay;
                _logger.LogWarning("{Channel} gönderimi başarısız ({Attempt}/{MaxAttempts}), {Delay} sonra tekrar denenecek: {Error}",
                    delivery.Channel, delivery.Attempts, RetryPolicy.MaxAttempts, delay, delivery.LastError);
            }
            else
            {
                delivery.Status = DeliveryStatus.Failed;
                _logger.LogError("{Channel} gönderimi {Attempts} denemeden sonra başarısız: {Error}", delivery.Channel, delivery.Attempts, delivery.LastError);
            }
        }

        delivery.ProcessingStartedAt = null;
        // Sonuç, kapanış sırasında bile yazılsın (aksi halde gönderilmiş mail Processing'de kalıp tekrar gider).
        await db.SaveChangesAsync(CancellationToken.None);
    }
}
