using MultiShop.DtoLayer.CatalogDtos.AboutDtos;

namespace MultiShop.WebUI.Services.CatalogServices.AboutServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface IAboutService
    {
        Task<List<ResultAboutDto>> GetAllAboutAsync();

        Task<string?> CreateAboutAsync(CreateAboutDto createAboutDto);

        Task<string?> UpdateAboutAsync(UpdateAboutDto updateAboutDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteAboutAsync(string id);

        Task<UpdateAboutDto> GetByIdAboutAsync(string id);
    }
}
