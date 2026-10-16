using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Dtos.FeatureSliderDtos;

namespace MultiShop.Catalog.Services.FeatureSliderServices
{
    public interface IFeatureSliderService
    {
        Task<List<ResultFeatureSliderDto>> GetAllFeatureSliderAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateFeatureSliderAsync(CreateFeatureSliderDto createFeatureSliderDto);

        Task UpdateFeatureSliderAsync(UpdateFeatureSliderDto updateFeatureSliderDto);

        Task DeleteFeatureSliderAsync(string id);

        Task<GetByIdFeatureSliderDto> GetByIdFeatureSliderAsync(string id);
    }
}
