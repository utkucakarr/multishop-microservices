using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Cargo.BusinessLayer.Abstract;
using MultiShop.Cargo.DataAccessLayer.Concrete;
using MultiShop.Cargo.DtoLayer.Dtos.CargoCompanyDtos;
using MultiShop.Cargo.EntityLayer.Concrete;
using System.Data.SqlTypes;
using System.Security.Cryptography.X509Certificates;

namespace MultiShop.Cargo.WebApi.Controllers
{
    // Kargo yönetimi yalnızca admin panelinden yapılır; müşteri tarafında kargo takibi yok.
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class CargoCompaniesController : ControllerBase
    {
        private readonly ICargoCompanyService _cargoCompanyService;
        private readonly CargoContext _context;

        public CargoCompaniesController(ICargoCompanyService cargoCompanyService, CargoContext context)
        {
            _cargoCompanyService = cargoCompanyService;
            _context = context;
        }

        [HttpGet]
        public IActionResult CargoCompanyList()
        {
            var values = _cargoCompanyService.TGetAll();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateCargoCompany(CreateCargoCompanyDto createCargoCompanyDto)
        {
            CargoCompany cargoCompany = new CargoCompany()
            {
                CargoCompanyName = createCargoCompanyDto.CargoCompanyName,
            };
            _cargoCompanyService.TInsert(cargoCompany);
            return Ok("Kargo şirketi başarıyla oluşturuldu");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCargoCompany(int id)
        {
            // Eskiden firma silinince bağlı kargo kayıtları da (cascade) siliniyordu; gönderisi olan firma artık silinemez.
            if (await _context.CargoDetails.AnyAsync(x => x.CargoCompanyId == id))
                throw new ConflictException("Bu firmaya bağlı gönderiler var; firma silinemez.");
            _cargoCompanyService.TDelete(id);
            return Ok("Kargo şirketi başarıyla silindi");
        }

        [HttpGet("{id}")]
        public IActionResult GetCargoCompanyById(int id)
        {
            var value = _cargoCompanyService.TGetById(id);
            return Ok(value);
        }

        /// <summary>Firmayı varsayılan yapar: yeni gönderilere otomatik atanır. Aynı anda yalnızca bir firma varsayılandır.</summary>
        [HttpPost("{id:int}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            if (!await _context.CargoCompanies.AnyAsync(x => x.CargoCompanyId == id))
                throw new NotFoundException("Kargo firması", id);

            await _context.CargoCompanies.ExecuteUpdateAsync(set => set.SetProperty(x => x.IsDefault, x => x.CargoCompanyId == id));
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCargoCompany(UpdateCargoCompanyDto updateCargoCompanyDto)
        {
            CargoCompany cargoCompany = new CargoCompany()
            {
                CargoCompanyId = updateCargoCompanyDto.CargoCompanyId,
                CargoCompanyName = updateCargoCompanyDto.CargoCompanyName,
                // Update tüm alanları yazar: varsayılan firmanın adı düzenlenince varsayılanlığı kaybolmasın.
                IsDefault = await _context.CargoCompanies.Where(x => x.CargoCompanyId == updateCargoCompanyDto.CargoCompanyId)
                    .Select(x => x.IsDefault).FirstOrDefaultAsync(),
            };
            _cargoCompanyService.TUpdate(cargoCompany);
            return Ok("Kargo şirketi başarıyla güncellendi");
        }
    }
}
