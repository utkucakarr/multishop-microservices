using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MultiShop.Catalog.IntegrationTests.Infrastructure;
using MultiShop.Contracts.Catalog;

namespace MultiShop.Catalog.IntegrationTests
{
    /// <summary>
    /// Saga'nın stok komutları (KT-12) gerçek MongoDB'de, Mongo outbox transaction'ı içinde: ayır / kesinleştir / bırak,
    /// hepsi-ya-da-hiçbiri ve tekrar gelen komut. Docker gerekir.
    /// </summary>
    [Collection(CatalogApiCollection.Name)]
    public class StockReservationTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

        private readonly HttpClient _admin;
        private readonly ITestHarness _harness;
        private readonly Guid _orderNumber = Guid.NewGuid();

        public StockReservationTests(CatalogApiFactory factory)
        {
            _admin = factory.CreateClientAs(TestUser.Admin);
            _harness = factory.Services.GetRequiredService<ITestHarness>();
        }

        [Fact]
        public async Task Reserving_holds_stock_without_reducing_it()
        {
            var productId = await _admin.CreateProductAsync(await _admin.CreateCategoryAsync(), stock: 10);

            await SendAsync(CatalogQueues.ReserveStock, new ReserveStock(_orderNumber, new[] { new ReserveStockItem(productId, 3) }));
            await WaitForPublishedAsync<StockReserved>();

            var product = await _admin.GetProductAsync(productId);
            product.AvailableStock.Should().Be(10);
            product.ReservedStock.Should().Be(3);
        }

        [Fact]
        public async Task Reservation_is_all_or_nothing()
        {
            var categoryId = await _admin.CreateCategoryAsync();
            var enough = await _admin.CreateProductAsync(categoryId, stock: 10, name: "Kazak");
            var notEnough = await _admin.CreateProductAsync(categoryId, stock: 1, name: "Atkı");

            await SendAsync(CatalogQueues.ReserveStock, new ReserveStock(_orderNumber,
                new[] { new ReserveStockItem(enough, 2), new ReserveStockItem(notEnough, 5) }));
            var failed = await WaitForPublishedAsync<StockReservationFailed>();

            failed.Reason.Should().Contain("Atkı").And.Contain("kalan 1");
            (await _admin.GetProductAsync(enough)).ReservedStock.Should().Be(0, "bir satır yetmeyince hiçbir ürüne dokunulmaz");
        }

        [Fact]
        public async Task Commit_reduces_stock_and_release_returns_it()
        {
            var categoryId = await _admin.CreateCategoryAsync();
            var paidProduct = await _admin.CreateProductAsync(categoryId, stock: 10);
            await ReserveAsync(_orderNumber, paidProduct, 4);

            await SendAsync(CatalogQueues.CommitStock, new CommitStock(_orderNumber));
            await WaitForConsumedAsync<CommitStock>(_orderNumber);

            var paid = await _admin.GetProductAsync(paidProduct);
            paid.AvailableStock.Should().Be(6);
            paid.ReservedStock.Should().Be(0);

            var cancelledOrder = Guid.NewGuid();
            var cancelledProduct = await _admin.CreateProductAsync(categoryId, stock: 10);
            await ReserveAsync(cancelledOrder, cancelledProduct, 4);

            await SendAsync(CatalogQueues.ReleaseStock, new ReleaseStock(cancelledOrder));
            await WaitForConsumedAsync<ReleaseStock>(cancelledOrder);

            var cancelled = await _admin.GetProductAsync(cancelledProduct);
            cancelled.AvailableStock.Should().Be(10);
            cancelled.ReservedStock.Should().Be(0);
        }

        [Fact]
        public async Task Repeated_reserve_command_reserves_only_once()
        {
            var productId = await _admin.CreateProductAsync(await _admin.CreateCategoryAsync(), stock: 10);

            await ReserveAsync(_orderNumber, productId, 3);
            await SendAsync(CatalogQueues.ReserveStock, new ReserveStock(_orderNumber, new[] { new ReserveStockItem(productId, 3) }));
            await WaitForConsumedAsync<ReserveStock>(_orderNumber, count: 2);

            (await _admin.GetProductAsync(productId)).ReservedStock.Should().Be(3);
        }

        private async Task ReserveAsync(Guid orderNumber, string productId, int quantity)
        {
            await SendAsync(CatalogQueues.ReserveStock, new ReserveStock(orderNumber, new[] { new ReserveStockItem(productId, quantity) }));
            await WaitForConsumedAsync<ReserveStock>(orderNumber);
        }

        private async Task SendAsync<T>(string queue, T message) where T : class
        {
            var endpoint = await _harness.Bus.GetSendEndpoint(new Uri($"queue:{queue}"));
            await endpoint.Send(message);
        }

        // Harness'ın "bus boşta" sinyali bir kez tetiklenince sıfırlanmadığı için sonuç süre dolana kadar yoklanır
        // (bkz. Order saga testleri). Event Mongo outbox'tan teslim edilir; StockEventProbe onu işleyince burada görünür.
        private async Task<T> WaitForPublishedAsync<T>() where T : class
        {
            var deadline = DateTime.UtcNow + Timeout;
            while (DateTime.UtcNow < deadline)
            {
                var message = _harness.Consumed.Select<T>().Select(x => x.Context.Message)
                    .FirstOrDefault(x => (Guid)typeof(T).GetProperty("OrderNumber")!.GetValue(x)! == _orderNumber);
                if (message is not null)
                    return message;
                await Task.Delay(50);
            }
            throw new TimeoutException($"{typeof(T).Name} yayınlanmadı.");
        }

        private async Task WaitForConsumedAsync<T>(Guid orderNumber, int count = 1) where T : class
        {
            var deadline = DateTime.UtcNow + Timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (_harness.Consumed.Select<T>(x => (Guid)typeof(T).GetProperty("OrderNumber")!.GetValue(x.Context.Message)! == orderNumber).Count() >= count)
                    return;
                await Task.Delay(50);
            }
            throw new TimeoutException($"{count} adet {typeof(T).Name} işlenmedi.");
        }
    }
}
