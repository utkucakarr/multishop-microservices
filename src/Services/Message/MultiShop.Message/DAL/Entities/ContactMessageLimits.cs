namespace MultiShop.Message.DAL.Entities
{
    /// <summary>İletişim mesajı alan uzunlukları; hem veritabanı kolonları hem istek doğrulaması bunları kullanır.</summary>
    public static class ContactMessageLimits
    {
        public const int NameSurname = 100;
        public const int Email = 254;
        public const int Subject = 200;
        public const int Message = 4000;
    }
}
