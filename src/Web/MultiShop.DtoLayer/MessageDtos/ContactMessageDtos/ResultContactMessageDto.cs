namespace MultiShop.DtoLayer.MessageDtos.ContactMessageDtos
{
    public class ResultContactMessageDto
    {
        public int ContactMessageId { get; set; }

        public string NameSurname { get; set; }

        public string Email { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        /// <summary>UTC; ekranda <c>ToLocalTime()</c> ile gösterilir.</summary>
        public DateTime SendDate { get; set; }
    }
}
