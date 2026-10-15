using MultiShop.Catalog.Dtos.FeatureDtos;

namespace MultiShop.Catalog.Services.FeatureServices
{
    public interface IFeatureService
    {
        Task<List<ResultFeatureDto>> GetAllFeatureAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateFeatureAsync(CreateFeatureDto createFeatureDto);

        Task UpdateFeatureAsync(UpdateFeatureDto updateFeatureDto);

        Task DeleteFeatureAsync(string id);

        Task<GetByIdFeatureDto> GetByIdFeatureAsync(string id);
    }
}
