using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.ProductDetailDtos;
using MultiShop.Catalog.Services.ProductDetailServices;

namespace MultiShop.Catalog.Controllers
{
    // Todo: [Authorize]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDetailsController : ControllerBase
    {
        private readonly IProductDetailService _productDetailService;

        public ProductDetailsController (IProductDetailService productDetailService)
        {
            _productDetailService = productDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetailList()
        {
            var productDetails = await _productDetailService.GetAllProductDetailAsync();
            return Ok(productDetails);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPorductDetailById(string id)
        {
            var productDetailId = await _productDetailService.GetByIdProductDetailAsync(id);
            return Ok(productDetailId);
        }

        [HttpGet("GetProductDetailByProductId/{id}")]
        public async Task<IActionResult> GetProductDetailByProductId(string id)
        {
            var productDetailId = await _productDetailService.GetByProductIdProductDetailAsync(id);
            return Ok(productDetailId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductDetail(CreateProductDetailDto createProductDetailDto)
        {
            await _productDetailService.CreateProductDetailAsync(createProductDetailDto);
            return Ok("Ürün detayı başarıyla eklendi");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProductDetail(string id)
        {
            await _productDetailService.DeleteProductDetailAsync(id);
            return Ok("Ürün detayı başarıyla silindi");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProductDetail(UpdateProductDetailDto updateProductDetailDto)
        {
            await _productDetailService.UpdateProductDetailAsyn(updateProductDetailDto);
            return Ok("Ürün detayı başarıyla güncellendi");
        }
    }
}