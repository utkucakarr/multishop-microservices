using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;

namespace MultiShop.WebUI.Services.CatalogServices.CategoryServices
{
    // Okuma metotları servis hata dönerse ServiceApiException fırlatır (bkz. ServiceExceptionMiddleware);
    // yazma metotları başarılıysa null, değilse kullanıcıya gösterilecek hata mesajını döner.
    public interface ICategoryService
    {
        Task<List<ResultCategoryDto>> GetAllCategoryAsync();

        Task<string?> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

        Task<string?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı.</summary>
        Task<string?> DeleteCategoryAsync(string id);

        Task<UpdateCategoryDto> GetByIdCategoryAsync(string id);
    }
}
