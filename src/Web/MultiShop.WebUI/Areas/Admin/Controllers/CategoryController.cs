using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        void CategoryViewBagList()
        {
            ViewBag.v1 = "Anasayfa";
            ViewBag.v2 = "Kategoriler";
            ViewBag.v3 = "Kategori Listesi";
            ViewBag.v0 = "Kategori İşemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            CategoryViewBagList();

            var values = await _categoryService.GetAllCategoryAsync();
            return View(values);
        }

        [Route("CreateCategory")]
        [HttpGet]
        public IActionResult CreateCategory()
        {
            CategoryViewBagList();
            return View();
        }

        [Route("CreateCategory")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            var error = await _categoryService.CreateCategoryAsync(createCategoryDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                CategoryViewBagList();
                return View(createCategoryDto);
            }
            TempData["SuccessMessage"] = "Kategori eklendi.";
            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }

        [Route("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var error = await _categoryService.DeleteCategoryAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Kategori silindi.";
            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }

        [Route("UpdateCategory/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateCategory(string id)
        {
            CategoryViewBagList();
            var values = await _categoryService.GetByIdCategoryAsync(id);
            return View(values);
        }

        [Route("UpdateCategory/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            var error = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                CategoryViewBagList();
                return View(updateCategoryDto);
            }
            TempData["SuccessMessage"] = "Kategori güncellendi.";
            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }
    }
}