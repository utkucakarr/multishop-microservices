namespace MultiShop.Message.Dtos
{
    /// <summary>Admin dashboard'unun ve üst menüdeki zarfın Message kısmı.</summary>
    public class MessageStatisticsDto
    {
        /// <summary>Toplam iletişim formu mesajı.</summary>
        public int ContactMessages { get; set; }

        /// <summary>Okunmamış iletişim formu mesajı.</summary>
        public int Unread { get; set; }
    }
}
