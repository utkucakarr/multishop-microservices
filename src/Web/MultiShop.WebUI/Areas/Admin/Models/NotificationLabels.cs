namespace MultiShop.WebUI.Areas.Admin.Models
{
    // Notification servisinden metin olarak gelen kanal/durum değerlerinin ekrandaki karşılıkları.
    public static class NotificationLabels
    {
        public static string Channel(string channel) => channel switch
        {
            "InApp" => "Panel içi",
            "Email" => "Mail",
            "Sms" => "SMS",
            _ => channel
        };

        public static string Status(string status) => status switch
        {
            "Pending" => "Bekliyor",
            "Processing" => "Gönderiliyor",
            "Sent" => "Gönderildi",
            "Failed" => "Başarısız",
            "Simulated" => "Simüle edildi",
            _ => status
        };

        // Bootstrap 3 label sınıfı.
        public static string StatusCss(string status) => status switch
        {
            "Sent" => "label-success",
            "Failed" => "label-danger",
            "Simulated" => "label-warning",
            "Processing" => "label-info",
            _ => "label-default"
        };
    }
}
