using MultiShop.DtoLayer.CargoDtos.ShipmentDtos;

namespace MultiShop.WebUI.Models
{
    /// <summary>
    /// Cargo'nun gönderi durumlarının (API metin olarak döner) Türkçe adı ve etiket rengi; admin gönderi sayfalarında ortak.
    /// </summary>
    public static class ShipmentStatusDisplay
    {
        /// <summary>Admin filtresindeki sıra.</summary>
        public static readonly IReadOnlyList<string> All = new[] { ShipmentStatuses.Preparing, ShipmentStatuses.Shipped, ShipmentStatuses.Delivered };

        public static string Text(string? status) => status switch
        {
            ShipmentStatuses.Preparing => "Hazırlanıyor",
            ShipmentStatuses.Shipped => "Kargoda",
            ShipmentStatuses.Delivered => "Teslim edildi",
            _ => status ?? "-"
        };

        /// <summary>Admin paneli (Bootstrap 3): <c>label label-*</c>.</summary>
        public static string LabelClass(string? status) => status switch
        {
            ShipmentStatuses.Preparing => "label-warning",
            ShipmentStatuses.Shipped => "label-info",
            ShipmentStatuses.Delivered => "label-success",
            _ => "label-default"
        };
    }
}
