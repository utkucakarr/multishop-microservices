using MassTransit;
using MultiShop.Catalog.Services.StockServices;
using MultiShop.Contracts.Catalog;

namespace MultiShop.Catalog.Consumers
{
    // Sipariş saga'sının (Order) komutları. Her consumer Mongo outbox transaction'ı içinde çalışır (Program.cs):
    // stok değişikliği, rezervasyon kaydı, inbox kaydı ve yayınlanan event tek transaction'da; aynı komut iki kez gelirse
    // inbox atlar, rezervasyon kaydı da ikinci kez ayırmayı engeller.

    /// <summary>Kuyruk: <c>catalog-reserve-stock</c>. Cevap: StockReserved ya da StockReservationFailed.</summary>
    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly StockReservationService _stock;
        private readonly ILogger<ReserveStockConsumer> _logger;

        public ReserveStockConsumer(StockReservationService stock, ILogger<ReserveStockConsumer> logger)
        {
            _stock = stock;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            var message = context.Message;
            var result = await _stock.ReserveAsync(message.OrderNumber, message.Items, context.CancellationToken);

            if (result.Succeeded)
                await context.Publish(new StockReserved(message.OrderNumber));
            else
            {
                _logger.LogInformation("Sipariş {OrderNumber} için stok ayrılamadı: {Reason}", message.OrderNumber, result.FailureReason);
                await context.Publish(new StockReservationFailed(message.OrderNumber, result.FailureReason!));
            }
        }
    }

    /// <summary>Kuyruk: <c>catalog-commit-stock</c>. Ödeme alındı; ayrılan stok eldeki stoktan düşer.</summary>
    public class CommitStockConsumer : IConsumer<CommitStock>
    {
        private readonly StockReservationService _stock;

        public CommitStockConsumer(StockReservationService stock)
        {
            _stock = stock;
        }

        public Task Consume(ConsumeContext<CommitStock> context)
            => _stock.CommitAsync(context.Message.OrderNumber, context.CancellationToken);
    }

    /// <summary>Kuyruk: <c>catalog-release-stock</c>. Telafi: iptal edilen siparişin ayrılan stoğu serbest kalır.</summary>
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        private readonly StockReservationService _stock;

        public ReleaseStockConsumer(StockReservationService stock)
        {
            _stock = stock;
        }

        public Task Consume(ConsumeContext<ReleaseStock> context)
            => _stock.ReleaseAsync(context.Message.OrderNumber, context.CancellationToken);
    }
}
