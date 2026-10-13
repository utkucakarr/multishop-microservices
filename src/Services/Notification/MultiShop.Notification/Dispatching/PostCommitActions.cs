namespace MultiShop.Notification.Dispatching;

/// <summary>
/// Veritabanı commit'inden SONRA yapılacak işler (SignalR push, DeliveryProcessor sinyali). Commit'ten önce yapılsalar ve
/// transaction geri alınsa, istemci var olmayan bir bildirim görürdü ("hayalet bildirim").
/// Dispatcher işi buraya ekler; <see cref="PostCommitInterceptor"/> gerçek commit'te çalıştırır, rollback'te atar.
/// Event consumer'da commit'i MassTransit inbox'ı consumer bittikten sonra yaptığı için Dispatcher'ın kendi
/// SaveChanges'i commit anlamına gelmez; bu yüzden işler SaveChanges'e değil commit'e bağlı.
/// </summary>
public sealed class PostCommitActions
{
    private readonly List<Func<CancellationToken, Task>> _actions = new();
    private readonly ILogger<PostCommitActions> _logger;

    public PostCommitActions(ILogger<PostCommitActions> logger)
    {
        _logger = logger;
    }

    public void Add(Func<CancellationToken, Task> action) => _actions.Add(action);

    /// <summary>Bekleyen işleri sırayla çalıştırır. Veri zaten kaydedildi: bir işin hatası diğerlerini ve çağıranı etkilemez.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var actions = _actions.ToList();
        _actions.Clear();

        foreach (var action in actions)
        {
            try
            {
                await action(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Commit sonrası iş başarısız (kayıt kalıcı).");
            }
        }
    }

    public void Discard() => _actions.Clear();
}
