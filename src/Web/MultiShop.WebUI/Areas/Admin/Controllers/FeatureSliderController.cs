using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.FeatureSliderDtos;
using MultiShop.WebUI.Services.CatalogServices.FeatureSliderServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/FeatureSlider")]
    public class FeatureSliderController : Controller
    {
        private readonly IFeatureSliderService _featureSliderService;

        public FeatureSliderController(IFeatureSliderService featureSliderService)
        {
            _featureSliderService = featureSliderService;
        }

        void FeatureSliderViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Öne Çıkan Görseller";
            ViewBag.v3 = "Öne Çıkan Görsel Listesi";
            ViewBag.v0 = "Öne Çıkan Slider Görsel İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            FeatureSliderViewBagList();

            var valıues = await _featureSliderService.GetAllFeatureSliderAsync();
            return View(valıues);
        }

        [Route("CreateFeatureSlider")]
        [HttpGet]
        public IActionResult CreateFeatureSlider()
        {
            FeatureSliderViewBagList();
            return View();
        }

        [Route("CreateFeatureSlider")]
        [HttpPost]
        public async Task<IActionResult> CreateFeatureSlider(CreateFeatureSliderDto createFeatureSliderDto)
        {
            createFeatureSliderDto.Status = false;
            var error = await _featureSliderService.CreateFeatureSliderAsync(createFeatureSliderDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                FeatureSliderViewBagList();
                return View(createFeatureSliderDto);
            }
            TempData["SuccessMessage"] = "Slider eklendi.";
            return RedirectToAction("Index", "FeatureSlider", new { area = "Admin" });
        }

        [HttpPost]
        [Route("DeleteFeatureSlider/{id}")]
        public async Task<IActionResult> DeleteFeatureSlider(string id)
        {
            var error = await _featureSliderService.DeleteFeatureSliderAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Slider silindi.";
            return RedirectToAction("Index", "FeatureSlider", new { area = "Admin" });
        }

        [Route("UpdateFeatureSlider/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateFeatureSlider(string id)
        {
            FeatureSliderViewBagList();
            var values = await _featureSliderService.GetByIdFeatureSliderAsync(id);
            return View(values);
        }

        [Route("UpdateFeatureSlider/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateFeatureSlider(UpdateFeatureSliderDto updateFeatureSliderDto)
        {
            var error = await _featureSliderService.UpdateFeatureSliderAsync(updateFeatureSliderDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                FeatureSliderViewBagList();
                return View(updateFeatureSliderDto);
            }
            TempData["SuccessMessage"] = "Slider güncellendi.";
            return RedirectToAction("Index", "FeatureSlider", new { area = "Admin" });
        }
    }
}
