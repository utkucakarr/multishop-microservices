namespace MultiShop.Contracts.Catalog;

// Sipariş saga'sının (Order) Catalog'a gönderdiği KOMUTLAR. Tek alıcısı Catalog'dur; Send ile doğrudan kuyruğa gider.
// Stok modeli: ayrılmış (ReservedStock) ödeme gelince kesinleşir, iptalde serbest bırakılır (docs/02 → Saga kararları).

/// <summary>Satırlardaki adetleri ayır: hepsi ya da hiçbiri. Cevap <see cref="StockReserved"/> veya <see cref="StockReservationFailed"/>.</summary>
public record ReserveStock(Guid OrderNumber, IReadOnlyList<ReserveStockItem> Items);

public record ReserveStockItem(string ProductId, int Quantity);

/// <summary>Ödeme alındı: siparişin ayrılmış stoğunu kesinleştir (eldeki stoktan düş). Cevap beklenmez.</summary>
public record CommitStock(Guid OrderNumber);

/// <summary>Sipariş iptal: ayrılmış stoğu serbest bırak (telafi işlemi). Rezervasyon yoksa bir şey yapılmaz. Cevap beklenmez.</summary>
public record ReleaseStock(Guid OrderNumber);
