namespace MultiShop.DtoLayer.CommentDtos
{
    /// <summary>Ürün sayfasındaki onaylı yorum (Comment API: <c>GET comments/product/{productId}</c>).</summary>
    public class ResultProductCommentDto
    {
        public int UserCommentId { get; set; }

        public string NameSurname { get; set; } = string.Empty;

        public string CommentDetail { get; set; } = string.Empty;

        public int Rating { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
