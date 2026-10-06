using MultiShop.DtoLayer.CatalogDtos.FeatureDtos;

namespace MultiShop.WebUI.Services.CatalogServices.FeautureServices
{
    public interface IFeatureService
    {
        Task<List<ResultFeatureDto>> GetAllFeatureAsync();

        Task CreateFeatureAsync(CreateFeatureDto createFeatureDto);

        Task UpdateFeatureAsync(UpdateFeatureDto updateFeatureDto);

        Task DeleteFeatureAsync(string id);

        Task<UpdateFeatureDto> GetByIdFeatureAsync(string id);
    }
}
