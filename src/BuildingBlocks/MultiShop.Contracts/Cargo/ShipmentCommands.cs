namespace MultiShop.Contracts.Cargo;

/// <summary>
/// Ödeme alındı, siparişin gönderisini aç (Order saga'sının Cargo'ya KOMUTU; tek alıcı, <c>Send</c>).
/// Alıcı bilgisi mesajda (event-carried state transfer): Cargo Order'a sormaz; adres sipariş anındaki haliyle kargolanır.
/// Aynı sipariş için ikinci komut yeni gönderi açmaz.
/// </summary>
public record CreateShipment(Guid OrderNumber, int OrderingId, string UserId, ShipmentRecipient Recipient);

/// <summary>Teslimat adresi ve iletişim (Order'daki adresin kopyası). Kişisel veri: Contracts README → kural 1 istisnası.</summary>
public record ShipmentRecipient(
    string Name,
    string Surname,
    string Email,
    string Phone,
    string AddressLine1,
    string? AddressLine2,
    string District,
    string City,
    string Country,
    string? ZipCode);
