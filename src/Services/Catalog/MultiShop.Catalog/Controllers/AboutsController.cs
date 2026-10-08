using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.AboutDtos;
using MultiShop.Catalog.Services.AboutServices;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.Controllers
{
    // Okuma: ziyaretçi token'ı da yeterli; yazma (POST/PUT/DELETE) yalnızca Admin.
    [Authorize(Policy = MultiShopPolicies.Read)]
    [Route("api/[controller]")]
    [ApiController]
    public class AboutsController : ControllerBase
    {
        private readonly IAboutService _aboutService;

        public AboutsController(IAboutService aboutService)
        {
            _aboutService = aboutService;
        }

        [HttpGet]
        public async Task<IActionResult> AboutList()
        {
            var abouts = await _aboutService.GetallAboutAsync();
            return Ok(abouts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAboutById(string id)
        {
            var aboutId = await _aboutService.GetByIdAboutAsync(id);
            return Ok(aboutId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateAbout(CreateAboutDto createAboutDto)
        {
            await _aboutService.CreateAboutAsync(createAboutDto);
            return Ok("Hakkımda alanı başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteAbout(string id)
        {
            await _aboutService.DeleteAboutAsync(id);
            return Ok("Hakkımda alanı başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateAbout(UpdateAboutDto updateAboutDto)
        {
            await _aboutService.UpdateAboutAsync(updateAboutDto);
            return Ok("Hakkımda alanı başarıyla güncellendi");
        }
    }
}