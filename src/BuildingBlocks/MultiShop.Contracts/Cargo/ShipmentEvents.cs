namespace MultiShop.Contracts.Cargo;

// Cargo'daki gönderinin durumu değişti (admin panelinden). Order saga'sı siparişin durumunu ilerletir,
// Notification müşteriye haber verir; Cargo kimin dinlediğini bilmez.

/// <summary>Gönderi kargo firmasına verildi.</summary>
/// <param name="UserId">Siparişin sahibi; bildirim bu kullanıcıya gider.</param>
/// <param name="CustomerEmail">Bildirim için (event-carried state transfer; Notification kimseye sormaz).</param>
public record ShipmentDispatched(
    Guid OrderNumber,
    string TrackingNumber,
    string CargoCompanyName,
    string UserId,
    string CustomerName,
    string CustomerEmail,
    DateTime DispatchedAt);

/// <summary>Gönderi müşteriye teslim edildi; sipariş tamamlanır.</summary>
public record ShipmentDelivered(Guid OrderNumber, string TrackingNumber, DateTime DeliveredAt);
