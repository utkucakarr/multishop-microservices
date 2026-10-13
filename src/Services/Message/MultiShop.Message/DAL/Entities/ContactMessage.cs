namespace MultiShop.Message.DAL.Entities
{
    /// <summary>Sitedeki iletişim formundan ziyaretçinin mağazaya gönderdiği mesaj.</summary>
    public class ContactMessage
    {
        public int ContactMessageId { get; set; }

        public string NameSurname { get; set; }

        public string Email { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        /// <summary>Gönderim zamanı (UTC); sunucu atar.</summary>
        public DateTime SendDate { get; set; }
    }
}
