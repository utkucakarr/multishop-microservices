namespace MultiShop.Message.Dtos
{
    public class CreateMessageDto
    {
        // Yok sayılır; gönderen her zaman token'daki kullanıcıdır.
        public string? SenderId { get; set; }

        public string ReveiverId { get; set; }

        public string Subject { get; set; }

        public string MessageDetail { get; set; }

        public bool IsRead { get; set; }

        public DateTime MessageDate { get; set; }
    }
}
