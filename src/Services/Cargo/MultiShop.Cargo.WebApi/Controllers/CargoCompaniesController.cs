using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Cargo.BusinessLayer.Services;
using MultiShop.Cargo.DtoLayer.Dtos.CargoCompanyDtos;

namespace MultiShop.Cargo.WebApi.Controllers
{
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class CargoCompaniesController : ControllerBase
    {
        private readonly CargoCompanyService _cargoCompanyService;

        public CargoCompaniesController(CargoCompanyService cargoCompanyService)
        {
            _cargoCompanyService = cargoCompanyService;
        }

        [HttpGet]
        public async Task<IActionResult> CargoCompanyList()
            => Ok(await _cargoCompanyService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCargoCompanyById(int id)
            => Ok(await _cargoCompanyService.GetAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateCargoCompany(SaveCargoCompanyDto dto)
        {
            var id = await _cargoCompanyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetCargoCompanyById), new { id }, new { cargoCompanyId = id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCargoCompany(int id, SaveCargoCompanyDto dto)
        {
            await _cargoCompanyService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>Gönderisi olan firma silinemez (409).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveCargoCompany(int id)
        {
            await _cargoCompanyService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>Firmayı varsayılan yapar: yeni gönderilere otomatik atanır. Aynı anda yalnızca bir firma varsayılandır.</summary>
        [HttpPost("{id:int}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            await _cargoCompanyService.SetDefaultAsync(id);
            return NoContent();
        }
    }
}
