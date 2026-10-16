namespace MultiShop.DtoLayer.CommentDtos
{
    /// <summary>Yeni yorum (Comment API). Ad ve e-posta giriş yapan kullanıcının profilinden; durum ve tarihi API atar.</summary>
    public class CreateCommentDto
    {
        public string ProductId { get; set; } = string.Empty;

        public string NameSurname { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CommentDetail { get; set; } = string.Empty;

        public int Rating { get; set; }
    }
}
