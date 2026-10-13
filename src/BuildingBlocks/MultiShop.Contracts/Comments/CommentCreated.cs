namespace MultiShop.Contracts.Comments;

/// <summary>
/// Comment bir ürüne yeni yorum kaydettiğinde yayınlar. Notification dinler ve admin'e bildirim düşer (KT-10).
/// Yorum metni ve yazanın e-postası bilerek yok: bildirim için gerekmiyor, admin yorumu panelden okur.
/// </summary>
/// <param name="CommentId">Comment servisindeki yorum numarası.</param>
/// <param name="ProductId">Yorumun yapıldığı ürün (Catalog'daki id).</param>
/// <param name="NameSurname">Yorumu yazanın formdaki adı.</param>
/// <param name="Rating">Verilen puan.</param>
/// <param name="CreatedAt">Yorumun kaydedildiği an (UTC).</param>
public record CommentCreated(int CommentId, string ProductId, string NameSurname, int Rating, DateTime CreatedAt);
