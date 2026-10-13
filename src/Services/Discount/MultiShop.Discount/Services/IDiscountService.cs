using MultiShop.Discount.Dtos;

namespace MultiShop.Discount.Services
{
    public interface IDiscountService
    {
        Task<List<ResultDiscountCouponDto>> GetAllDiscountCouponAsync();

        Task CreateDiscountCouponAsync(CreateDiscountCouponDto createCouponDto);

        Task UpdateDiscountCouponAync(UpdateDiscountCouponDto updateCouponDto);

        Task DeleteDiscountCouponAsync(int id);

        Task<GetByIdDiscountCouponDto> GetByIdDiscountCouponAsync(int id);

        Task<ResultDiscountCouponDto> GetCodeDetailByCodeAsync(string code);

        Task<int> GetDiscountCouponCountRate(string code);

        /// <summary>Aktif ve süresi dolmamış kuponu döner; yoksa <see cref="BuildingBlocks.Exceptions.NotFoundException"/>.</summary>
        Task<CouponValidationDto> ValidateCouponAsync(string code);

        Task<DiscountStatisticsDto> GetStatisticsAsync();
    }
}
