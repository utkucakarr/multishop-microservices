using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Services.ProductServices;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Catalog.Controllers
{
    // Okuma: ziyaretçi token'ı da yeterli; yazma (POST/PUT/DELETE) yalnızca Admin.
    [Authorize(Policy = MultiShopPolicies.Read)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductList()
        {
            var products = await _productService.GetAllProductAsync();
            return Ok(products);
        }

        // Bir sepette bulunabilecek farklı ürün sayısının üst sınırı; tek istekte sınırsız id sorgulanmasın.
        private const int MaxIdsPerRequest = 100;

        /// <summary>Basket servisi sepeti fiyatlarken çağırır: <c>?ids=a&amp;ids=b</c>. Olmayan ürünler yanıtta yer almaz.</summary>
        [HttpGet("by-ids")]
        public async Task<IActionResult> GetProductPricesByIds([FromQuery] string[] ids)
        {
            if (ids.Length > MaxIdsPerRequest)
                throw new BadRequestException($"Tek istekte en fazla {MaxIdsPerRequest} ürün sorgulanabilir.");

            var values = await _productService.GetPricesByIdsAsync(ids);
            return Ok(values);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(string id)
        {

            var productId = await _productService.GetByIdProductAsync(id);
            return Ok(productId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            var id = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProductById), new { id }, new { id });
        }

        /// <summary>Admin: eldeki stoğu ayarlar (ürün güncelleme formu stok alanı taşımaz).</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(string id, UpdateProductStockDto updateProductStockDto)
        {
            await _productService.UpdateStockAsync(id, updateProductStockDto.AvailableStock);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
            await _productService.UpdateProductAsync(updateProductDto);
            return NoContent();
        }

        [HttpGet("ProductListWithCategory")]
        public async Task<IActionResult> ProductListWithCategory()
        {
            var values = await _productService.GetProductsWithCategoryAsync();
            return Ok(values);
        }

        [HttpGet("GetProductsWithCategoryByCategoryId/{id}")]
        public async Task<IActionResult> GetProductsWithCategoryByCategoryId(string id)
        {
            var values = await _productService.GetProductsWithCategoryByCategoryIdAsync(id);
            return Ok(values);
        }

    }
}