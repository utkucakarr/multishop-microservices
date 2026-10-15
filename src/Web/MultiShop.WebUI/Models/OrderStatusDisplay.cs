namespace MultiShop.WebUI.Models
{
    /// <summary>
    /// Order'ın sipariş durumlarının (API metin olarak döner) Türkçe adı ve rozet rengi; tüm sayfalarda aynı görünsün diye tek yerde.
    /// </summary>
    public static class OrderStatusDisplay
    {
        public const string Pending = "Pending";
        public const string AwaitingPayment = "AwaitingPayment";
        public const string Paid = "Paid";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        /// <summary>Admin filtresindeki sıra.</summary>
        public static readonly IReadOnlyList<string> All = new[] { Pending, AwaitingPayment, Paid, Completed, Cancelled };

        public static string Text(string? status) => status switch
        {
            Pending => "Hazırlanıyor",
            AwaitingPayment => "Ödeme bekleniyor",
            Paid => "Ödendi",
            Completed => "Tamamlandı",
            Cancelled => "İptal edildi",
            _ => status ?? "-"
        };

        /// <summary>Site ve kullanıcı paneli (Bootstrap 4): <c>badge badge-*</c>.</summary>
        public static string BadgeClass(string? status) => status switch
        {
            Pending => "badge-secondary",
            AwaitingPayment => "badge-warning",
            Paid => "badge-success",
            Completed => "badge-primary",
            Cancelled => "badge-danger",
            _ => "badge-light"
        };

        /// <summary>Admin paneli (Bootstrap 3): <c>label label-*</c>.</summary>
        public static string LabelClass(string? status) => status switch
        {
            Pending => "label-default",
            AwaitingPayment => "label-warning",
            Paid => "label-success",
            Completed => "label-primary",
            Cancelled => "label-danger",
            _ => "label-default"
        };

        /// <summary>Ödemesi alınmamış sipariş: ödenebilir ya da iptal edilebilir.</summary>
        public static bool IsOpen(string? status) => status is Pending or AwaitingPayment;
    }
}
