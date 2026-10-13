namespace MultiShop.DtoLayer.NotificationDtos
{
    public class NotificationDetailDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public string? ReadBy { get; set; }
        public List<DeliveryDto> Deliveries { get; set; } = new();
    }

    // Kanal ve durum metin olarak gelir: Channel = "Email" | "Sms", Status = "Pending" | "Processing" | "Sent" | "Failed" | "Simulated".
    public class DeliveryDto
    {
        public Guid Id { get; set; }
        public string Channel { get; set; } = string.Empty;
        /// <summary>Maskeli adres.</summary>
        public string Recipient { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public DateTimeOffset? NextAttemptAt { get; set; }
        public string? LastError { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public bool CanRetry { get; set; }
    }
}
