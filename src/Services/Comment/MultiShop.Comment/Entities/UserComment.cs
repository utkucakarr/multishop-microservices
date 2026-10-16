namespace MultiShop.Comment.Entities
{
    public class UserComment
    {
        public int UserCommentId { get; set; }

        /// <summary>Yorumu yazan kullanıcı (token'daki <c>sub</c>). KT-14 öncesi yorumlarda boş.</summary>
        public string? UserId { get; set; }

        public string NameSurname { get; set; } = string.Empty;

        /// <summary>Kullanılmıyor (WebUI eskiden "test" yazıyordu); eski kayıtlar için sütun duruyor.</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Yalnızca admin görür; ürün sayfasındaki yorum listesinde dönmez.</summary>
        public string Email { get; set; } = string.Empty;

        public string CommentDetail { get; set; } = string.Empty;

        public int Rating { get; set; }

        /// <summary>Sunucuda atanır (UTC).</summary>
        public DateTime CreatedDate { get; set; }

        public CommentStatus Status { get; set; }

        public string ProductId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Moderasyon durumu (COM-08). Sayılar eski <c>bool Status</c> ile uyumlu: migration'da false → Pending (0), true → Approved (1).
    /// </summary>
    public enum CommentStatus
    {
        Pending = 0,   // Yeni yorum: admin onayı bekliyor, sitede görünmez
        Approved = 1,  // Sitede görünür
        Rejected = 2   // Sitede görünmez, admin listesinde kalır
    }

    /// <summary>Alan sınırları; veritabanı şeması ve istek doğrulaması aynı değerleri kullanır.</summary>
    public static class CommentLimits
    {
        public const int NameSurnameMaxLength = 100;
        public const int EmailMaxLength = 256;
        public const int DetailMaxLength = 1000;
        public const int ProductIdMaxLength = 50;
        public const int UserIdMaxLength = 450;
        public const int MinRating = 1;
        public const int MaxRating = 5;
    }
}
