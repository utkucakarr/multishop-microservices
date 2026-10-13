namespace MultiShop.Discount.Entities
{
    /// <summary>Kupon alanlarının sınırları; veritabanı şeması ve DTO doğrulaması aynı değerleri kullanır.</summary>
    public static class CouponLimits
    {
        public const int CodeMaxLength = 50;
        public const int MinRate = 1;
        public const int MaxRate = 100;
    }
}
