namespace MultiShop.DtoLayer.NotificationDtos
{
    public class NotificationSettingsDto
    {
        public List<NotificationRuleDto> Rules { get; set; } = new();
        public List<NotificationRecipientDto> Recipients { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    // Hem listelemede hem "Kaydet" formunda kullanılır (DisplayName güncellemede yok sayılır).
    public class NotificationRuleDto
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool InAppEnabled { get; set; }
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
    }

    public class NotificationRecipientDto
    {
        public Guid Id { get; set; }
        /// <summary>"Email" | "Sms"</summary>
        public string Channel { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNotificationRecipientDto
    {
        /// <summary>"Email" | "Sms"</summary>
        public string Channel { get; set; } = "Email";
        public string Address { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
