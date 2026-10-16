using System.ComponentModel.DataAnnotations;
using MultiShop.Comment.Entities;

namespace MultiShop.Comment.Dtos
{
    /// <summary>
    /// Yeni yorum. Durum, tarih ve kullanıcı istekten alınmaz (COM-01): yorum her zaman onay bekler, tarih sunucuda,
    /// kullanıcı token'dan. Ad ve e-postayı WebUI giriş yapan kullanıcının profilinden doldurur.
    /// </summary>
    public class CreateCommentDto
    {
        [Required, MaxLength(CommentLimits.ProductIdMaxLength)]
        public string ProductId { get; set; } = string.Empty;

        [Required, MaxLength(CommentLimits.NameSurnameMaxLength)]
        public string NameSurname { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(CommentLimits.EmailMaxLength)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yorum boş olamaz.")]
        [MaxLength(CommentLimits.DetailMaxLength, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string CommentDetail { get; set; } = string.Empty;

        [Range(CommentLimits.MinRating, CommentLimits.MaxRating, ErrorMessage = "Puan 1 ile 5 arasında olmalı.")]
        public int Rating { get; set; }
    }

    /// <summary>Ürün sayfasındaki yorum: yazanın e-postası ve kullanıcı id'si herkese açık listede dönmez.</summary>
    public record ProductCommentDto(int UserCommentId, string NameSurname, string CommentDetail, int Rating, DateTime CreatedDate);

    /// <summary>Admin listesindeki yorum.</summary>
    public record CommentDto(
        int UserCommentId,
        string ProductId,
        string? UserId,
        string NameSurname,
        string Email,
        string CommentDetail,
        int Rating,
        DateTime CreatedDate,
        CommentStatus Status);

    /// <summary>Sayfalı liste (COM-04).</summary>
    public record CommentPageDto(IReadOnlyList<CommentDto> Items, int TotalCount, int Page, int PageSize);
}
