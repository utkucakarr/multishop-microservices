using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.AboutDtos;
using MultiShop.WebUI.Services.CatalogServices.AboutServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/About")]
    public class AboutController : Controller
    {
        private readonly IAboutService _aboutService;

        public AboutController(IAboutService aboutService)
        {
            _aboutService = aboutService;
        }

        void AboutViewBagList()
        {
            ViewBag.v1 = "Anasayfa";
            ViewBag.v2 = "Hakkımızda";
            ViewBag.v3 = "Hakkımızda Listesi";
            ViewBag.v0 = "Hakkımızda İşemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            AboutViewBagList();

            var values = await _aboutService.GetAllAboutAsync();
            return View(values);
        }

        [HttpGet]
        [Route("CreateAbout")]
        public IActionResult CreateAbout()
        {
            AboutViewBagList();
            return View();
        }

        [HttpPost]
        [Route("CreateAbout")]
        public async Task<IActionResult> CreateAbout(CreateAboutDto createAboutDto)
        {
            var error = await _aboutService.CreateAboutAsync(createAboutDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                AboutViewBagList();
                return View(createAboutDto);
            }
            TempData["SuccessMessage"] = "Hakkımızda kaydı eklendi.";
            return RedirectToAction("Index", "About", new { area = "Admin" });
        }

        [Route("DeleteAbout/{id}")]
        public async Task<IActionResult> DeleteAbout(string id)
        {
            var error = await _aboutService.DeleteAboutAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Hakkımızda kaydı silindi.";
            return RedirectToAction("Index", "About", new { area = "Admin" });

        }

        [HttpGet]
        [Route("UpdateAbout/{id}")]
        public async Task<IActionResult> UpdateAbout(string id)
        {
            AboutViewBagList();
            var values = await _aboutService.GetByIdAboutAsync(id);
            return View(values);
        }

        [HttpPost]
        [Route("UpdateAbout/{id}")]
        public async Task<IActionResult> UpdateAbout(UpdateAboutDto updateAboutDto)
        {
            var error = await _aboutService.UpdateAboutAsync(updateAboutDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                AboutViewBagList();
                return View(updateAboutDto);
            }
            TempData["SuccessMessage"] = "Hakkımızda kaydı güncellendi.";
            return RedirectToAction("Index", "About", new { area = "Admin" });
        }
    }
}
