using System.ComponentModel.DataAnnotations;
using MultiShop.Discount.Entities;

namespace MultiShop.Discount.Dtos
{
    public class UpdateDiscountCouponDto
    {
        public int CouponId { get; set; }

        [Required, StringLength(CouponLimits.CodeMaxLength)]
        public string Code { get; set; }

        [Range(CouponLimits.MinRate, CouponLimits.MaxRate)]
        public int Rate { get; set; }

        public bool IsActive { get; set; }

        public DateTime ValidDate { get; set; }
    }
}
