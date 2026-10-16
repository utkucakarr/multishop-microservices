using MultiShop.DtoLayer.CommentDtos;

namespace MultiShop.WebUI.Models
{
    /// <summary>Yorum moderasyon durumlarının Türkçe adı ve admin etiket rengi.</summary>
    public static class CommentStatusDisplay
    {
        /// <summary>Admin filtresindeki sıra.</summary>
        public static readonly IReadOnlyList<string> All = new[] { CommentStatuses.Pending, CommentStatuses.Approved, CommentStatuses.Rejected };

        public static string Text(string? status) => status switch
        {
            CommentStatuses.Pending => "Onay bekliyor",
            CommentStatuses.Approved => "Onaylandı",
            CommentStatuses.Rejected => "Reddedildi",
            _ => status ?? "-"
        };

        /// <summary>Admin paneli (Bootstrap 3): <c>label label-*</c>.</summary>
        public static string LabelClass(string? status) => status switch
        {
            CommentStatuses.Pending => "label-warning",
            CommentStatuses.Approved => "label-success",
            CommentStatuses.Rejected => "label-danger",
            _ => "label-default"
        };
    }
}
