using MultiShop.DtoLayer.CatalogDtos.OfferDiscountDtos;

namespace MultiShop.WebUI.Services.CatalogServices.OfferDiscountServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface IOfferDiscountService
    {
        Task<List<ResultOfferDiscountDto>> GetAllOfferDiscountAsync();

        Task<string?> CreateOfferDiscountAsync(CreateOfferDiscountDto createOfferDiscountDto);

        Task<string?> UpdateOfferDiscountAsync(UpdateOfferDiscountDto updateOfferDiscountDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteOfferDiscountAsync(string id);

        Task<UpdateOfferDiscountDto> GetByIdOfferDiscountAsync(string id);
    }
}
