using MassTransit;
using MultiShop.Contracts.Catalog;

namespace MultiShop.Catalog.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Catalog'un stok event'lerini (Order saga'sının yerine) dinler; bir şey yapmaz. Event'ler Mongo outbox'tan ham gövde
    /// olarak gönderildiği için harness onları ancak bir consumer işleyince tipiyle (<c>Consumed.Select&lt;StockReserved&gt;</c>) görür.
    /// </summary>
    public class StockEventProbe : IConsumer<StockReserved>, IConsumer<StockReservationFailed>
    {
        public Task Consume(ConsumeContext<StockReserved> context) => Task.CompletedTask;

        public Task Consume(ConsumeContext<StockReservationFailed> context) => Task.CompletedTask;
    }
}
