namespace MultiShop.Contracts.Catalog;

/// <summary>Siparişin tüm satırları için stok ayrıldı.</summary>
public record StockReserved(Guid OrderNumber);

/// <summary>Stok ayrılamadı; hiçbir satır ayrılmadı.</summary>
/// <param name="Reason">Kullanıcıya gösterilebilir sebep, ör. "Kahve Makinesi için yeterli stok yok."</param>
public record StockReservationFailed(Guid OrderNumber, string Reason);
