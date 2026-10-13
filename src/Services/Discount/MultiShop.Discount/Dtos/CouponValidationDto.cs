namespace MultiShop.Discount.Dtos
{
    /// <summary>Geçerli (aktif, süresi dolmamış) kuponun sepette uygulanacak bilgisi. Basket servisi kullanır.</summary>
    public class CouponValidationDto
    {
        public string Code { get; set; }

        /// <summary>İndirim yüzdesi (1–100).</summary>
        public int Rate { get; set; }
    }
}
