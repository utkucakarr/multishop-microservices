using MultiShop.Discount.Dtos;

namespace MultiShop.Discount.Services
{
    public interface IDiscountService
    {
        Task<List<ResultDiscountCouponDto>> GetAllDiscountCouponAsync();

        /// <returns>Oluşan kuponun id'si. Aynı kod varsa <see cref="BuildingBlocks.Exceptions.ConflictException"/>.</returns>
        Task<int> CreateDiscountCouponAsync(CreateDiscountCouponDto createCouponDto);

        /// <summary>Kupon yoksa <see cref="BuildingBlocks.Exceptions.NotFoundException"/>.</summary>
        Task UpdateDiscountCouponAsync(UpdateDiscountCouponDto updateCouponDto);

        /// <summary>Kupon yoksa <see cref="BuildingBlocks.Exceptions.NotFoundException"/>.</summary>
        Task DeleteDiscountCouponAsync(int id);

        /// <summary>Kupon yoksa <see cref="BuildingBlocks.Exceptions.NotFoundException"/>.</summary>
        Task<GetByIdDiscountCouponDto> GetByIdDiscountCouponAsync(int id);

        /// <summary>Aktif ve süresi dolmamış kuponu döner; yoksa <see cref="BuildingBlocks.Exceptions.NotFoundException"/>.</summary>
        Task<CouponValidationDto> ValidateCouponAsync(string code);

        Task<DiscountStatisticsDto> GetStatisticsAsync();
    }
}
