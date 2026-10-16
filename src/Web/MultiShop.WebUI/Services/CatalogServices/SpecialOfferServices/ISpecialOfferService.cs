using MultiShop.DtoLayer.CatalogDtos.SpecialOfferDtos;

namespace MultiShop.WebUI.Services.CatalogServices.SpecialOfferServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface ISpecialOfferService
    {
        Task<List<ResultSpecialOfferDto>> GetAllSpecialOfferAsync();

        Task<string?> CreateSpecialOfferAsync(CreateSpecialOfferDto createSpecialOfferDto);

        Task<string?> UpdateSpecialOfferAsync(UpdateSpecialOfferDto updateSpecialOfferDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteSpecialOfferAsync(string id);

        Task<UpdateSpecialOfferDto> GetByIdSpecialOfferAsync(string id);
    }
}
