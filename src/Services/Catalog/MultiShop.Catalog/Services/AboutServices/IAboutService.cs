using MultiShop.Catalog.Dtos.AboutDtos;

namespace MultiShop.Catalog.Services.AboutServices
{
    public interface IAboutService
    {
        Task<List<ResultAboutDto>> GetallAboutAsync();

        Task CreateAboutAsync(CreateAboutDto createAboutDto);

        Task UpdateAboutAsync(UpdateAboutDto updateAboutDto);

        Task DeleteAboutAsync(string id);

        Task<GetByIdAboutDto> GetByIdAboutAsync(string id);
    }
}
