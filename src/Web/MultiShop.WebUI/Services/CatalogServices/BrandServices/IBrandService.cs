using MultiShop.DtoLayer.CatalogDtos.BrandDtos;

namespace MultiShop.WebUI.Services.CatalogServices.BrandServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface IBrandService
    {
        Task<List<ResultBrandDto>> GetAllBrandAsync();

        Task<string?> CreateBrandAsync(CreateBrandDto createBrandDto);

        Task<string?> UpdateBrandAsync(UpdateBrandDto updateBrandDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı (ör. markaya bağlı ürün varsa).</summary>
        Task<string?> DeleteBrandAsync(string id);

        Task<UpdateBrandDto> GetByIdBrandAsync(string id);
    }
}
