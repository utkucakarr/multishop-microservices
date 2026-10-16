namespace MultiShop.Order.Application.Features.Mediator.Validators
{
    /// <summary>
    /// Adres alanlarının sınırları. Sütunlar nvarchar(max), ama adres sipariş ödenince Cargo'ya kopyalanır (CreateShipment)
    /// ve oradaki sütunlar sınırlı: alıcı adı "Ad Soyad" 200, e-posta 256, telefon 50 karakter.
    /// </summary>
    public static class AddressLimits
    {
        public const int NameMaxLength = 50;
        public const int EmailMaxLength = 256;
        public const int PhoneMaxLength = 20;
        public const int PlaceMaxLength = 50;
        public const int LineMaxLength = 200;
        public const int ZipCodeMaxLength = 10;
        public const int DescriptionMaxLength = 500;
    }
}
