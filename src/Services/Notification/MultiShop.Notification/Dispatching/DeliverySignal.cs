using System.Threading.Channels;

namespace MultiShop.Notification.Dispatching;

/// <summary>
/// Dispatcher → DeliveryProcessor "yeni iş var" uyandırması (bellek içi, ephemeral). Yalnızca gecikmeyi azaltır;
/// sinyal kaybolsa da (ör. servis yeniden başladı) işlemcinin periyodik taraması Pending satırları bulur.
/// </summary>
public sealed class DeliverySignal
{
    // Kapasite 1: art arda gelen sinyaller tek uyandırmaya indirgenir.
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _channel.Writer.TryWrite(true);

    /// <summary>Sinyal gelene ya da süre dolana kadar bekler.</summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            await _channel.Reader.ReadAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Süre doldu: periyodik tarama zamanı.
        }
    }
}
