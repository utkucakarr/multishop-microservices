namespace MultiShop.DtoLayer.NotificationDtos
{
    // Notification servisinin bildirim kutusu satırı (GET services/notification/notifications).
    public class NotificationListItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public string? ReadBy { get; set; }
        public int DeliveryCount { get; set; }
        public int FailedDeliveryCount { get; set; }
    }
}
