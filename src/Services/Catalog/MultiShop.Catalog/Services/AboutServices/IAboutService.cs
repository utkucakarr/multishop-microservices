using MultiShop.Catalog.Dtos.AboutDtos;

namespace MultiShop.Catalog.Services.AboutServices
{
    public interface IAboutService
    {
        Task<List<ResultAboutDto>> GetAllAboutAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateAboutAsync(CreateAboutDto createAboutDto);

        Task UpdateAboutAsync(UpdateAboutDto updateAboutDto);

        Task DeleteAboutAsync(string id);

        Task<GetByIdAboutDto> GetByIdAboutAsync(string id);
    }
}
