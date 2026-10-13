using MultiShop.DtoLayer.CatalogDtos.BrandDtos;

namespace MultiShop.WebUI.Services.CatalogServices.BrandServices
{
    public interface IBrandService
    {
        Task<List<ResultBrandDto>> GetAllBrandAsync();

        Task CreateBrandAsync(CreateBrandDto createBrandDto);

        Task UpdateBrandAsync(UpdateBrandDto updateBrandDto);

        /// <summary>Başarılıysa null, değilse Catalog'un hata mesajı (ör. markaya bağlı ürün varsa).</summary>
        Task<string?> DeleteBrandAsync(string id);

        Task<UpdateBrandDto> GetByIdBrandAsync(string id);
    }
}
