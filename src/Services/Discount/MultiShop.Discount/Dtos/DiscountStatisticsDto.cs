namespace MultiShop.Discount.Dtos
{
    /// <summary>Admin dashboard'unun Discount kısmı. Aktif = <c>IsActive</c> ve süresi dolmamış (gün bazında).</summary>
    public class DiscountStatisticsDto
    {
        public int CouponCount { get; set; }
        public int ActiveCouponCount { get; set; }
    }
}
