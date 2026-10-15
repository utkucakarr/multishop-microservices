using MultiShop.Catalog.Dtos.CategoryDtos;

namespace MultiShop.Catalog.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<ResultCategoryDto>> GetAllCategoryAsync();

        /// <returns>Oluşan kaydın id'si.</returns>
        Task<string> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

        Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);

        Task DeleteCategoryAsync(string id);

        Task<GetByIdCategoryDto> GetByIdCategoryAsync(string id);
    }
}
