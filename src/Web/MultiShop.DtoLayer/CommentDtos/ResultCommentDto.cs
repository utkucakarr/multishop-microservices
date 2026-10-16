namespace MultiShop.DtoLayer.CommentDtos
{
    /// <summary>Admin yorum listesi satırı.</summary>
    public class ResultCommentDto
    {
        public int UserCommentId { get; set; }

        public string ProductId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public string NameSurname { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CommentDetail { get; set; } = string.Empty;

        public int Rating { get; set; }

        public DateTime CreatedDate { get; set; }

        /// <summary>Pending, Approved, Rejected (<see cref="CommentStatuses"/>).</summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Sayfalı admin listesi (Comment API: <c>GET comments?status=&amp;page=</c>).</summary>
    public class CommentPageDto
    {
        public List<ResultCommentDto> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    /// <summary>Comment API'nin moderasyon durumları (metin olarak döner).</summary>
    public static class CommentStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
