namespace MultiShop.Contracts.Baskets;

/// <summary>
/// Kullanıcı sepeti onayladığında Basket yayınlar; Order'daki sipariş saga'sını başlatır (KT-12).
/// Satırlar ve tutarlar Basket'in sunucuda hesapladığı değerlerdir (event-carried state transfer): Order ne Basket'e
/// ne Catalog'a sorar, sipariş tutarı istemciden gelmez (ORD-02).
/// </summary>
/// <param name="OrderNumber">Siparişin kimliği; Basket üretir, saga'nın Correlation ID'si, tüm sipariş mesajlarında aynı.</param>
/// <param name="UserId">Siparişi veren kullanıcı (token'daki <c>sub</c>).</param>
/// <param name="AddressId">Order'daki teslimat adresi.</param>
/// <param name="TotalPrice">Genel toplam: ara toplam − indirim + KDV + kargo (tahsil edilecek tutar).</param>
public record BasketCheckedOut(
    Guid OrderNumber,
    string UserId,
    int AddressId,
    IReadOnlyList<BasketCheckedOutItem> Items,
    string? CouponCode,
    int DiscountRate,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ShippingFee,
    decimal TotalPrice,
    DateTime CheckedOutAt);

/// <summary>Sipariş satırı; fiyat Catalog'un o anki fiyatı.</summary>
public record BasketCheckedOutItem(string ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
