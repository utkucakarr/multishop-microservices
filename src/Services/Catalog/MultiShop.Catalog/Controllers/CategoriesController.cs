using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Services.CategoryServices;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.Controllers
{
    // Okuma: ziyaretçi token'ı da yeterli; yazma (POST/PUT/DELETE) yalnızca Admin.
    [Authorize(Policy = MultiShopPolicies.Read)]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> CategoryList()
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            var categoryId = await _categoryService.GetByIdCategoryAsync(id);
            return Ok(categoryId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            await _categoryService.CreateCategoryAsync(createCategoryDto);
            return Ok("Katregori başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok("Kategori başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory (UpdateCategoryDto updateCategoryDto)
        {
            await _categoryService.UpdateCategoryAsync(updateCategoryDto);
            return Ok("Kategori başarıyla güncellendi");
        }
    }
}