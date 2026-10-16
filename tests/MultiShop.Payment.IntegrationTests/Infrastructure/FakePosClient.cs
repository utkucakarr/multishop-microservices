using MultiShop.Payment.Services.GarantiPosServices;

namespace MultiShop.Payment.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Sahte banka: istenen sonucu döner, istenirse <see cref="HoldNextSale"/> ile cevabı test bırakana kadar bekletir
    /// (bu sırada ikinci ödeme ya da iptal denenir).
    /// </summary>
    public class FakePosClient : IPosClient
    {
        private readonly List<string> _orderIds = new();
        private TaskCompletionSource? _entered;
        private TaskCompletionSource? _release;

        public Func<GarantiPosResult> Result { get; set; } = () => new GarantiPosResult(true, null);

        public IReadOnlyList<string> OrderIds
        {
            get { lock (_orderIds) return _orderIds.ToList(); }
        }

        /// <summary>Sonraki satış isteği bankaya girince tamamlanan görev; cevap <see cref="Release"/> çağrılana kadar bekler.</summary>
        public Task HoldNextSale()
        {
            _entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _entered.Task;
        }

        public void Release() => _release?.TrySetResult();

        public async Task<GarantiPosResult> SaleAsync(string orderId, decimal amount, CardDetails card)
        {
            lock (_orderIds) _orderIds.Add(orderId);

            if (_release is { } release)
            {
                _entered!.TrySetResult();
                await release.Task;
            }

            return Result();
        }
    }
}
