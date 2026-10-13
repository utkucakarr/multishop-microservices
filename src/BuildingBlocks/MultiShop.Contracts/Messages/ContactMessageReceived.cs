namespace MultiShop.Contracts.Messages;

/// <summary>
/// Message servisi iletişim formundan yeni bir mesaj kaydettiğinde yayınlar. Notification dinler; admin'e panel içi bildirim ve mail gider (KT-10).
/// Mesaj metni bilerek yok: admin mesajı panelden okur. Gönderenin e-postası var çünkü admin'e giden mailde gösteriliyor
/// (kişisel veri: kuyruklara ve loglara yazılır; yeni bir alan eklemeden önce bunu göz önünde bulundurun).
/// </summary>
/// <param name="ContactMessageId">Message servisindeki mesaj numarası (admin panelindeki detay linki).</param>
/// <param name="NameSurname">Gönderenin formdaki adı.</param>
/// <param name="Email">Gönderenin formdaki e-postası.</param>
/// <param name="Subject">Mesajın konusu.</param>
/// <param name="SentAt">Mesajın kaydedildiği an (UTC).</param>
public record ContactMessageReceived(int ContactMessageId, string NameSurname, string Email, string Subject, DateTime SentAt);
