using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CargoDtos.CargoCompanyDtos;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.CargoServices.CargoCompanyServices;
using MultiShop.WebUI.Services.CargoServices.ShipmentServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Kargo firmaları ve gönderiler. Gönderiyi ödenen siparişe Cargo açar; admin kargoya verir ve teslim edildi işaretler,
    // sipariş durumu (Kargoda / Tamamlandı) bu olaylarla saga'da ilerler.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Cargo")]
    public class CargoController : Controller
    {
        private readonly ICargoCompanyService _cargoCompanyService;
        private readonly IShipmentService _shipmentService;

        public CargoController(ICargoCompanyService cargoCompanyService, IShipmentService shipmentService)
        {
            _cargoCompanyService = cargoCompanyService;
            _shipmentService = shipmentService;
        }

        [Route("CargoCompanyList")]
        public async Task<IActionResult> CargoCompanyList()
        {
            var values = await _cargoCompanyService.GetAllCargoCompanyAsync();
            return View(values);
        }

        [HttpGet]
        [Route("CreateCargoCompany")]
        public IActionResult CreateCargoCompany()
        {
            return View();
        }

        [HttpPost]
        [Route("CreateCargoCompany")]
        public async Task<IActionResult> CreateCargoCompany(CreateCargoCompanyDto createCargoCompanyDto)
        {
            await _cargoCompanyService.CreateCargoCompanyAsync(createCargoCompanyDto);
            return RedirectToAction("CargoCompanyList", "Cargo", new { Area = "Admin" });
        }

        [Route("DeleteCargoCompany/{id}")]
        public async Task<IActionResult> DeleteCargoCompany(int id)
        {
            await _cargoCompanyService.DeleteCargoCompanyAsync(id);
            return RedirectToAction("CargoCompanyList", "Cargo", new { Area = "Admin" });
        }

        [HttpGet]
        [Route("UpdateCargoCompany/{id}")]
        public async Task<IActionResult> UpdateCargoCompany(int id)
        {
            var values = await _cargoCompanyService.GetByIdCargoCompanyAsync(id);
            return View(values);
        }

        [HttpPost]
        [Route("UpdateCargoCompany")]
        public async Task<IActionResult> UpdateCargoCompany(UpdateCargoCompanyDto updateCargoCompanyDto)
        {
            await _cargoCompanyService.UpdateCargoCompanyAsync(updateCargoCompanyDto);
            return RedirectToAction("CargoCompanyList", "Cargo", new { Area = "Admin" });
        }

        [HttpPost]
        [Route("SetDefaultCargoCompany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultCargoCompany(int id)
        {
            var error = await _cargoCompanyService.SetDefaultCargoCompanyAsync(id);
            if (error is null)
                TempData["SuccessMessage"] = "Varsayılan kargo firması değişti; yeni gönderiler bu firmaya atanacak.";
            else
                TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(CargoCompanyList));
        }

        [Route("ShipmentList")]
        public async Task<IActionResult> ShipmentList(string? status)
        {
            ViewBag.status = ShipmentStatusDisplay.All.Contains(status) ? status : null;
            var values = await _shipmentService.GetAllAsync(ViewBag.status);
            return View(values);
        }

        [Route("ShipmentDetail/{id:int}")]
        public async Task<IActionResult> ShipmentDetail(int id)
        {
            var value = await _shipmentService.GetByIdAsync(id);
            if (value is null)
                return NotFound();
            return View(value);
        }

        [HttpPost]
        [Route("DispatchShipment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchShipment(int id, string? status, bool fromDetail)
        {
            var error = await _shipmentService.DispatchAsync(id);
            if (error is null)
                TempData["SuccessMessage"] = "Gönderi kargoya verildi; sipariş birkaç saniye içinde \"Kargoda\" olacak ve müşteriye takip numarası gidecek.";
            else
                TempData["ErrorMessage"] = error;
            return RedirectAfterShipmentAction(id, status, fromDetail);
        }

        [HttpPost]
        [Route("DeliverShipment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeliverShipment(int id, string? status, bool fromDetail)
        {
            var error = await _shipmentService.DeliverAsync(id);
            if (error is null)
                TempData["SuccessMessage"] = "Gönderi teslim edildi; sipariş birkaç saniye içinde tamamlanacak.";
            else
                TempData["ErrorMessage"] = error;
            return RedirectAfterShipmentAction(id, status, fromDetail);
        }

        // İşlem detay sayfasından yapıldıysa oraya, listeden yapıldıysa aynı filtreyle listeye dönülür.
        IActionResult RedirectAfterShipmentAction(int id, string? status, bool fromDetail)
            => fromDetail ? RedirectToAction(nameof(ShipmentDetail), new { id }) : RedirectToAction(nameof(ShipmentList), new { status });

    }
}
