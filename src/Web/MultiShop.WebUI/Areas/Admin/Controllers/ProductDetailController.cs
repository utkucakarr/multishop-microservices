using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductDetailServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Ürünün açıklama ve bilgi kaydı (Catalog ProductDetail). Ürün başına tek kayıt vardır;
    // henüz girilmemişse aynı form ilk kaydedişte kaydı oluşturur.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/ProductDetail")]
    public class ProductDetailController : Controller
    {
        private readonly IProductDetailService _productDetailService;
        private readonly IProductService _productService;

        public ProductDetailController(IProductDetailService productDetailService, IProductService productService)
        {
            _productDetailService = productDetailService;
            _productService = productService;
        }

        [Route("UpdateProductDetail/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateProductDetail(string id)
        {
            await SetPageInfoAsync(id);

            // Detay kaydı yoksa servis boş nesne döner; ProductId'yi doldurup boş formu gösteriyoruz.
            var detail = await _productDetailService.GetByProductIdProductDetailAsync(id);
            return View(new UpdateProductDetailDto
            {
                ProductDetailId = detail.ProductDetailId,
                ProductId = id,
                ShortDescription = detail.ShortDescription,
                LongDescription = detail.LongDescription,
                ProductInfo = detail.ProductInfo
            });
        }

        [Route("UpdateProductDetail/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProductDetail(UpdateProductDetailDto updateProductDetailDto)
        {
            if (!ModelState.IsValid)
            {
                await SetPageInfoAsync(updateProductDetailDto.ProductId);
                return View(updateProductDetailDto);
            }

            var error = string.IsNullOrEmpty(updateProductDetailDto.ProductDetailId)
                ? await _productDetailService.CreateProductDetailAsync(new CreateProductDetailDto
                {
                    ProductId = updateProductDetailDto.ProductId,
                    ShortDescription = updateProductDetailDto.ShortDescription,
                    LongDescription = updateProductDetailDto.LongDescription,
                    ProductInfo = updateProductDetailDto.ProductInfo
                })
                : await _productDetailService.UpdateProductDetailAsync(updateProductDetailDto);

            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                await SetPageInfoAsync(updateProductDetailDto.ProductId);
                return View(updateProductDetailDto);
            }

            return RedirectToAction("Index", "Product", new { area = "Admin" });
        }

        private async Task SetPageInfoAsync(string productId)
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Açıklama ve Bilgi Güncelleme Sayfası";
            ViewBag.v0 = "Ürün İşlemleri";

            var product = await _productService.GetByIdProductAsync(productId);
            ViewBag.ProductName = product?.ProductName;
        }
    }
}
