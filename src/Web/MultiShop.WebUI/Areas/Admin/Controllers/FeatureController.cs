using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.FeatureDtos;
using MultiShop.WebUI.Services.CatalogServices.FeautureServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Feature")]
    public class FeatureController : Controller
    {
        private readonly IFeatureService _featureService;

        public FeatureController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        void FeatureViewBagList()
        {
            ViewBag.v1 = "Anasayfa";
            ViewBag.v2 = "Öne Çıkan Alanlar";
            ViewBag.v3 = "Öne Çıkan Alan Listesi";
            ViewBag.v0 = "Ana Sayfa Öne Çıkan Alan İşemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            FeatureViewBagList();
            var values = await _featureService.GetAllFeatureAsync();
            return View(values);
        }

        [Route("CreateFeature")]
        [HttpGet]
        public IActionResult CreateFeature()
        {
            FeatureViewBagList();
            return View();
        }

        [Route("CreateFeature")]
        [HttpPost]
        public async Task<IActionResult> CreateFeature(CreateFeatureDto createFeatureDto)
        {
            var error = await _featureService.CreateFeatureAsync(createFeatureDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                FeatureViewBagList();
                return View(createFeatureDto);
            }
            TempData["SuccessMessage"] = "Özellik eklendi.";
            return RedirectToAction("Index", "Feature", new { area = "Admin" });
        }

        [Route("DeleteFeature/{id}")]
        public async Task<IActionResult> DeleteFeature(string id)
        {
            var error = await _featureService.DeleteFeatureAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Özellik silindi.";
            return RedirectToAction("Index", "Feature", new { area = "Admin" });
        }

        [Route("UpdateFeature/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateFeature(string id)
        {
            FeatureViewBagList();
            var values = await _featureService.GetByIdFeatureAsync(id);
            return View(values);
        }

        [Route("UpdateFeature/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateFeature(UpdateFeatureDto updateFeatureDto)
        {
            var error = await _featureService.UpdateFeatureAsync(updateFeatureDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                FeatureViewBagList();
                return View(updateFeatureDto);
            }
            TempData["SuccessMessage"] = "Özellik güncellendi.";
            return RedirectToAction("Index", "Feature", new { area = "Admin" });
        }
    }
}
