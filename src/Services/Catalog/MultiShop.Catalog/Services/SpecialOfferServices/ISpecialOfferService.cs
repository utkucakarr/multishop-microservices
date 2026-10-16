using MultiShop.Catalog.Dtos.SpecialOfferDtos;

namespace MultiShop.Catalog.Services.SpecialOfferServices
{
    public interface ISpecialOfferService
    {
        Task<List<ResultSpecialOfferDto>> GetAllSpecialOfferAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateSpecialOfferAsync(CreateSpecialOfferDto createSpecialOfferDto);

        Task UpdateSpecialOfferAsync(UpdateSpecialOfferDto updateSpecialOfferDto);

        Task DeleteSpecialOfferAsync(string id);

        Task<GetByIdSpecialOfferDto> GetByIdSpecialOfferAsync(string id);
    }
}
