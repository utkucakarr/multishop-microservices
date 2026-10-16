using MultiShop.DtoLayer.CatalogDtos.FeatureDtos;

namespace MultiShop.WebUI.Services.CatalogServices.FeautureServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface IFeatureService
    {
        Task<List<ResultFeatureDto>> GetAllFeatureAsync();

        Task<string?> CreateFeatureAsync(CreateFeatureDto createFeatureDto);

        Task<string?> UpdateFeatureAsync(UpdateFeatureDto updateFeatureDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteFeatureAsync(string id);

        Task<UpdateFeatureDto> GetByIdFeatureAsync(string id);
    }
}
