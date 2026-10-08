using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.FeatureDtos;
using MultiShop.Catalog.Services.FeatureServices;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.Controllers
{
    // Okuma: ziyaretçi token'ı da yeterli; yazma (POST/PUT/DELETE) yalnızca Admin.
    [Authorize(Policy = MultiShopPolicies.Read)]
    [Route("api/[controller]")]
    [ApiController]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeaturesController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        [HttpGet]
        public async Task<IActionResult> FeatureList()
        {
            var features = await _featureService.GetAllFeatureAsync();
            return Ok(features);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeatureById(string id)
        {
            var featureId = await _featureService.GetByIdFeatureAsync(id);
            return Ok(featureId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateFeature(CreateFeatureDto createFeatureDto)
        {
            await _featureService.CreateFeatureAsync(createFeatureDto);
            return Ok("Öne çıkan alan başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteFeature(string id)
        {
            await _featureService.DeleteFeatureAsync(id);
            return Ok("Öne çıkan alan başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateFeature(UpdateFeatureDto updateFeatureDto)
        {
            await _featureService.UpdateFeatureAsync(updateFeatureDto);
            return Ok("Öne çıkan alan başarıyla güncellendi");
        }
    }
}