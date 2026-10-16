using MultiShop.Catalog.Dtos.OfferDiscountDtos;

namespace MultiShop.Catalog.Services.OfferDiscountServices
{
    public interface IOfferDiscountService
    {
        Task<List<ResultOfferDiscountDto>> GetAllOfferDiscountAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateOfferDiscountAsync(CreateOfferDiscountDto createOfferDiscountDto);

        Task UpdateOfferDiscountAsync(UpdateOfferDiscountDto updateOfferDiscountDto);

        Task DeleteOfferDiscountAsync(string id);

        Task<GetByIdOfferDiscountDto> GetByIdOfferDiscountAsync(string id);
    }
}
