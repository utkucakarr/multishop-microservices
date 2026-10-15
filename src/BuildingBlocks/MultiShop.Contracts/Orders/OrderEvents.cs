namespace MultiShop.Contracts.Orders;

/// <summary>
/// Stok ayrıldı, sipariş ödeme bekliyor. Payment dinler ve "bu sipariş için şu tutar beklenir" kaydını açar;
/// kart bilgisi bu mesajda yok, kullanıcı kartı Payment'a doğrudan (senkron) gönderir.
/// </summary>
/// <param name="OrderingId">Order'daki sipariş numarası (bankaya giden sipariş kimliği ve ekranlar için).</param>
/// <param name="TotalPrice">Tahsil edilecek tutar; Payment istekteki tutara değil buna bakar.</param>
public record OrderAwaitingPayment(Guid OrderNumber, int OrderingId, string UserId, decimal TotalPrice);

/// <summary>
/// Ödeme alındı (sipariş Paid). Yan işler bunu dinler: Notification müşteriye onay maili ve admin'e bildirim gönderir.
/// Gönderi açmak saga'nın işi olduğu için Cargo bunu değil <c>CreateShipment</c> komutunu alır.
/// </summary>
/// <param name="CustomerEmail">Teslimat adresindeki e-posta (event-carried state transfer; Notification Identity'ye sormaz).</param>
public record OrderPaid(
    Guid OrderNumber,
    int OrderingId,
    string UserId,
    decimal TotalPrice,
    DateTime PaidAt,
    string CustomerName,
    string CustomerEmail);

/// <summary>Sipariş iptal edildi (stok yetersiz, ödeme denemeleri bitti, kullanıcı ya da admin iptal etti). Payment bekleyen ödemeyi kapatır.</summary>
public record OrderCancelled(Guid OrderNumber, string UserId, string Reason);
