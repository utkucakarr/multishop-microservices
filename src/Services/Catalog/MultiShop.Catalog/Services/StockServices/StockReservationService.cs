using MassTransit.MongoDbIntegration;
using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Entities;
using MultiShop.Catalog.Settings;
using MultiShop.Contracts.Catalog;

namespace MultiShop.Catalog.Services.StockServices
{
    public record StockReservationResult(bool Succeeded, string? FailureReason)
    {
        public static StockReservationResult Success { get; } = new(true, null);
        public static StockReservationResult Fail(string reason) => new(false, reason);
    }

    /// <summary>
    /// Sipariş saga'sının stok adımları (KT-12): ayır → (ödeme) → kesinleştir, iptalde serbest bırak.
    /// Her metot, consumer'ı saran MassTransit Mongo outbox'ının açtığı transaction'da (<see cref="MongoDbContext.Session"/>)
    /// çalışır: ürün güncellemeleri, rezervasyon kaydı, inbox kaydı ve yayınlanan event birlikte commit edilir ya da hiçbiri.
    /// </summary>
    public class StockReservationService
    {
        private readonly MongoDbContext _dbContext;
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<StockReservation> _reservations;
        private readonly ILogger<StockReservationService> _logger;

        public StockReservationService(MongoDbContext dbContext, IMongoDatabase database, IDatabaseSettings settings, ILogger<StockReservationService> logger)
        {
            _dbContext = dbContext;
            _products = database.GetCollection<Product>(settings.ProductCollectionName);
            _reservations = database.GetCollection<StockReservation>(settings.StockReservationCollectionName);
            _logger = logger;
        }

        private IClientSessionHandle Session => _dbContext.Session
            ?? throw new InvalidOperationException("Stok işlemleri MassTransit Mongo outbox transaction'ı içinde çalışmalı.");

        /// <summary>Hepsi ya da hiçbiri: bir satır bile karşılanamazsa hiçbir ürüne dokunulmaz.</summary>
        public async Task<StockReservationResult> ReserveAsync(Guid orderNumber, IReadOnlyCollection<ReserveStockItem> items, CancellationToken cancellationToken)
        {
            var existing = await FindReservationAsync(orderNumber, cancellationToken);
            if (existing is not null)
            {
                // Aynı komut tekrar geldi (inbox süresi dolduktan sonra yeniden teslim gibi): ikinci kez ayrılmaz.
                _logger.LogInformation("Sipariş {OrderNumber} için rezervasyon zaten var ({Status}).", orderNumber, existing.Status);
                return existing.Status == StockReservationStatus.Released
                    ? StockReservationResult.Fail("Sipariş iptal edilmiş.")
                    : StockReservationResult.Success;
            }

            var lines = items
                .GroupBy(x => x.ProductId)
                .Select(g => new StockReservationItem { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();
            if (lines.Count == 0 || lines.Any(x => x.Quantity <= 0))
                return StockReservationResult.Fail("Siparişte geçerli bir ürün satırı yok.");

            var ids = lines.Select(x => x.ProductId).Where(id => ObjectId.TryParse(id, out _)).ToList();
            var products = (await _products.Find(Session, p => ids.Contains(p.ProductId)).ToListAsync(cancellationToken))
                .ToDictionary(p => p.ProductId);

            // Önce hepsi kontrol edilir; yetmeyen ilk satır kullanıcıya gösterilecek sebep olur.
            foreach (var line in lines)
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                    return StockReservationResult.Fail("Siparişteki bir ürün artık satışta değil.");
                if (product.SellableStock < line.Quantity)
                    return StockReservationResult.Fail(
                        $"{product.ProductName} için yeterli stok yok (istenen {line.Quantity}, kalan {Math.Max(product.SellableStock, 0)}).");
            }

            foreach (var line in lines)
            {
                // Koşullu ve atomik: satılabilir adet hâlâ yeterliyse ayır. Aynı ürünü aynı anda ayıran başka bir sipariş
                // varsa MongoDB yazma çakışması verir; transaction geri alınır ve mesaj retry ile yeniden işlenir.
                var result = await _products.UpdateOneAsync(Session, SellableAtLeast(line.ProductId, line.Quantity),
                    Builders<Product>.Update.Inc(p => p.ReservedStock, line.Quantity), cancellationToken: cancellationToken);
                if (result.ModifiedCount == 0)
                    throw new InvalidOperationException($"Ürün {line.ProductId} için stok ayrılamadı; işlem yeniden denenecek.");
            }

            var now = DateTime.UtcNow;
            await _reservations.InsertOneAsync(Session, new StockReservation
            {
                OrderNumber = orderNumber,
                Items = lines,
                Status = StockReservationStatus.Reserved,
                CreatedAt = now,
                UpdatedAt = now,
            }, cancellationToken: cancellationToken);

            _logger.LogInformation("Sipariş {OrderNumber} için {LineCount} ürün satırında stok ayrıldı.", orderNumber, lines.Count);
            return StockReservationResult.Success;
        }

        /// <summary>Ödeme alındı: ayrılan adetler eldeki stoktan düşülür.</summary>
        public async Task CommitAsync(Guid orderNumber, CancellationToken cancellationToken)
        {
            var reservation = await FindReservationAsync(orderNumber, cancellationToken);
            if (reservation?.Status != StockReservationStatus.Reserved)
            {
                _logger.LogWarning("Sipariş {OrderNumber} kesinleştirilemedi: rezervasyon durumu {Status}.", orderNumber, reservation?.Status.ToString() ?? "yok");
                return;
            }

            foreach (var line in reservation.Items)
            {
                await _products.UpdateOneAsync(Session, p => p.ProductId == line.ProductId,
                    Builders<Product>.Update.Inc(p => p.AvailableStock, -line.Quantity).Inc(p => p.ReservedStock, -line.Quantity),
                    cancellationToken: cancellationToken);
            }

            await SetStatusAsync(orderNumber, StockReservationStatus.Committed, cancellationToken);
            _logger.LogInformation("Sipariş {OrderNumber} için stok kesinleşti.", orderNumber);
        }

        /// <summary>Sipariş iptal edildi (telafi): ayrılan adetler serbest bırakılır. Rezervasyon yoksa yapılacak bir şey yok.</summary>
        public async Task ReleaseAsync(Guid orderNumber, CancellationToken cancellationToken)
        {
            var reservation = await FindReservationAsync(orderNumber, cancellationToken);
            if (reservation?.Status != StockReservationStatus.Reserved)
            {
                // Committed: para çekilmiş bir sipariş; stok iadesi iade akışıyla (KT-13) yapılır.
                if (reservation?.Status == StockReservationStatus.Committed)
                    _logger.LogWarning("Sipariş {OrderNumber} ödenmiş; stok serbest bırakılmadı (iade akışı gerekir).", orderNumber);
                return;
            }

            foreach (var line in reservation.Items)
            {
                await _products.UpdateOneAsync(Session, p => p.ProductId == line.ProductId,
                    Builders<Product>.Update.Inc(p => p.ReservedStock, -line.Quantity), cancellationToken: cancellationToken);
            }

            await SetStatusAsync(orderNumber, StockReservationStatus.Released, cancellationToken);
            _logger.LogInformation("Sipariş {OrderNumber} için ayrılan stok serbest bırakıldı.", orderNumber);
        }

        private Task<StockReservation?> FindReservationAsync(Guid orderNumber, CancellationToken cancellationToken)
            => _reservations.Find(Session, x => x.OrderNumber == orderNumber).FirstOrDefaultAsync(cancellationToken)!;

        private Task SetStatusAsync(Guid orderNumber, StockReservationStatus status, CancellationToken cancellationToken)
            => _reservations.UpdateOneAsync(Session, x => x.OrderNumber == orderNumber,
                Builders<StockReservation>.Update.Set(x => x.Status, status).Set(x => x.UpdatedAt, DateTime.UtcNow),
                cancellationToken: cancellationToken);

        // { _id: id, $expr: { $gte: [ AvailableStock − (ReservedStock ?? 0), adet ] } } — eski dokümanlarda ReservedStock yok.
        private static FilterDefinition<Product> SellableAtLeast(string productId, int quantity)
            => Builders<Product>.Filter.Eq("_id", ObjectId.Parse(productId))
               & new BsonDocumentFilterDefinition<Product>(new BsonDocument("$expr", new BsonDocument("$gte", new BsonArray
               {
                   new BsonDocument("$subtract", new BsonArray { "$AvailableStock", new BsonDocument("$ifNull", new BsonArray { "$ReservedStock", 0 }) }),
                   quantity,
               })));
    }
}
