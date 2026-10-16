using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.ProductImageDtos;
using MultiShop.Catalog.Services.ProductImageServices;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Catalog.Controllers
{
    // Okuma: ziyaretçi token'ı da yeterli; yazma (POST/PUT/DELETE) yalnızca Admin.
    [Authorize(Policy = MultiShopPolicies.Read)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductImageList()
        {
            var productImages = await _productImageService.GetAllProductImageAsync();
            return Ok(productImages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductImageById(string id)
        {
            var productImagesId = await _productImageService.GetByIdProductImageAsync(id);
            return Ok(productImagesId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateProductImage(CreateProductImageDto createProductImageDto)
        {
            var id = await _productImageService.CreateProductImageAsync(createProductImageDto);
            return CreatedAtAction(nameof(GetProductImageById), new { id }, new { id });
        }

        // Dosya yükleme (multipart/form-data): dosya depolamaya yazılır, ürüne görsel olarak eklenir.
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadProductImage([FromForm] UploadProductImageDto uploadProductImageDto)
        {
            var productImage = await _productImageService.UploadProductImageAsync(uploadProductImageDto);
            return Ok(productImage);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut("{id}/main")]
        public async Task<IActionResult> SetMainProductImage(string id)
        {
            await _productImageService.SetMainProductImageAsync(id);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteProductImage(string id)
        {
            await _productImageService.DeleteProductImageAsync(id);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateProductImage(UpdateProductImageDto updateProductImageDto)
        {
            await _productImageService.UpdateProductImageAsync(updateProductImageDto);
            return NoContent();
        }

        [HttpGet("ProductImagesByProductId/{id}")]
        public async Task<IActionResult> ProductImagesByProductId(string id)
        {
            var values = await _productImageService.GetByProductIdProductImageAsync(id);
            return Ok(values);
        }
    }
}