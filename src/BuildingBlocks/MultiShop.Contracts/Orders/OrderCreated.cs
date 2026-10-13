namespace MultiShop.Contracts.Orders;

/// <summary>
/// Order bir siparişi oluşturduğunda yayınlar. Catalog dinler ve stok ayırır (KT-12).
/// Ödeme bilgisi bilerek yok: kart verisi hiçbir event'e yazılmaz; ödeme UI → Gateway → Payment senkron çağrısıyla yapılır.
/// </summary>
/// <param name="OrderId">Order servisindeki sipariş numarası.</param>
/// <param name="UserId">Siparişi veren kullanıcı (token'daki <c>sub</c>).</param>
/// <param name="TotalPrice">Sunucuda hesaplanmış toplam tutar.</param>
/// <param name="Items">Sipariş satırları.</param>
public record OrderCreated(int OrderId, string UserId, decimal TotalPrice, IReadOnlyList<OrderCreatedItem> Items);

/// <summary>Sipariş satırı: Catalog hangi üründen kaç adet stok ayıracağını buradan bilir.</summary>
public record OrderCreatedItem(string ProductId, int Quantity);
