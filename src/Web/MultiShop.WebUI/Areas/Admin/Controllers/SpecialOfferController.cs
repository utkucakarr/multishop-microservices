using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.SpecialOfferDtos;
using MultiShop.WebUI.Services.CatalogServices.SpecialOfferServices;
using System.Threading.Tasks;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/SpecialOffer")]
    public class SpecialOfferController : Controller
    {
        private readonly ISpecialOfferService _specialOfferService;

        public SpecialOfferController(ISpecialOfferService specialOfferService)
        {
            _specialOfferService = specialOfferService;
        }
        void SpecialOfferViewBagList()
        {
            ViewBag.v1 = "Anasayfa";
            ViewBag.v2 = "Özel Teklifler";
            ViewBag.v3 = "Özel Teklif ve Günün İndirim Listesi";
            ViewBag.v0 = "Özel Teklif İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            SpecialOfferViewBagList();
            var values = await _specialOfferService.GetAllSpecialOfferAsync();
            return View(values);
        }

        [Route("CreateSpecialOffer")]
        [HttpGet]
        public async Task<IActionResult> CreateSpecialOffer()
        {
            var offer = await _specialOfferService.GetAllSpecialOfferAsync();
            var offerCount = offer.Count();
            ViewBag.offerCount = offerCount;
            SpecialOfferViewBagList();
            return View();
        }

        [Route("CreateSpecialOffer")]
        [HttpPost]
        public async Task<IActionResult> CreateSpecialOffer(CreateSpecialOfferDto createSpecialOfferDto)
        {
            var offer = await _specialOfferService.GetAllSpecialOfferAsync();
            var offerCount = offer.Count();
            ViewBag.offerCount = offerCount;
            // Ana sayfa tasarımı en fazla 2 özel teklif gösterir; uyarı view'da offerCount'a göre gösterilir.
            if (offerCount >= 2)
            {
                SpecialOfferViewBagList();
                return View(createSpecialOfferDto);
            }

            var error = await _specialOfferService.CreateSpecialOfferAsync(createSpecialOfferDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                SpecialOfferViewBagList();
                return View(createSpecialOfferDto);
            }
            TempData["SuccessMessage"] = "Özel teklif eklendi.";
            return RedirectToAction("Index", "SpecialOffer", new { area = "Admin" });
        }

        [HttpPost]
        [Route("DeleteSpecialOffer/{id}")]
        public async Task<IActionResult> DeleteSpecialOffer(string id)
        {
            var error = await _specialOfferService.DeleteSpecialOfferAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Özel teklif silindi.";
            return RedirectToAction("Index", "SpecialOffer", new { area = "Admin" });
        }

        [Route("UpdateSpecialOffer/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateSpecialOffer(string id)
        {
            SpecialOfferViewBagList();
            var values = await _specialOfferService.GetByIdSpecialOfferAsync(id);
            return View(values);
        }

        [Route("UpdateSpecialOffer/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateSpecialOffer(UpdateSpecialOfferDto updateSpecialOfferDto)
        {
            var error = await _specialOfferService.UpdateSpecialOfferAsync(updateSpecialOfferDto);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                SpecialOfferViewBagList();
                return View(updateSpecialOfferDto);
            }
            TempData["SuccessMessage"] = "Özel teklif güncellendi.";
            return RedirectToAction("Index", "SpecialOffer", new { area = "Admin" });
        }
    }
}
