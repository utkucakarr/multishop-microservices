namespace MultiShop.Notification.Data;

/// <summary>Bilinen bildirim türleri; kurallar, şablonlar ve event consumer'lar (KT-10) bu anahtarları kullanır.</summary>
public static class NotificationTypes
{
    public const string ContactMessageReceived = "contact-message-received";
    public const string CommentCreated = "comment-created";

    // KT-13: sipariş akışı. Müşteriye ve admin'e giden bildirimler ayrı tür: kanalları ayrı açılıp kapatılır.
    public const string OrderPaid = "order-paid";
    public const string NewOrder = "new-order";
    public const string OrderShipped = "order-shipped";
    public const string Test = "test";
}
