using MultiShop.DtoLayer.CatalogDtos.FeatureSliderDtos;

namespace MultiShop.WebUI.Services.CatalogServices.FeatureSliderServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface IFeatureSliderService
    {
        Task<List<ResultFeatureSliderDto>> GetAllFeatureSliderAsync();

        Task<string?> CreateFeatureSliderAsync(CreateFeatureSliderDto createFeatureSliderDto);

        Task<string?> UpdateFeatureSliderAsync(UpdateFeatureSliderDto updateFeatureSliderDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteFeatureSliderAsync(string id);

        Task<UpdateFeatureSliderDto> GetByIdFeatureSliderAsync(string id);
    }
}
