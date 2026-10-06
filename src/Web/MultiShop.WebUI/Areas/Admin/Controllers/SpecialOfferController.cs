using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MultiShop.DtoLayer.CatalogDtos.SpecialOfferDtos;
using MultiShop.WebUI.Services.CatalogServices.SpecialOfferServices;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
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
            if (offerCount >= 2)
            {
                TempData["errorMessage"] = "En fazla 2 özel teklif eklenebilir.";
            }
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
            if (offerCount >= 2)
            {
                TempData["errorMessage"] = "En fazla 2 özel teklif eklenebilir.";
                return View(createSpecialOfferDto);
            }
            else
            {
                await _specialOfferService.CreateSpecialOfferAsync(createSpecialOfferDto);
                return RedirectToAction("Index", "SpecialOffer", new { area = "Admin" });
            }
        }

        [Route("DeleteSpecialOffer/{id}")]
        public async Task<IActionResult> DeleteSpecialOffer(string id)
        {
            await _specialOfferService.DeleteSpecialOfferAsync(id);
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
            await _specialOfferService.UpdateSpecialOfferAsync(updateSpecialOfferDto);
            return RedirectToAction("Index", "SpecialOffer", new { area = "Admin" });
        }
    }
}
