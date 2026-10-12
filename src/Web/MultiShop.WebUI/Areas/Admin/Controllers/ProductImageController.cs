using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.CatalogServices.ProductImageServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Bir ürünün görselleri: listeleme, dosya yükleme, ana görsel seçme, silme.
    // Dosyalar Catalog'un IFileStorage'ına (yerel disk ya da GCS) yazılır.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/ProductImage")]
    public class ProductImageController : Controller
    {
        private const string ErrorKey = "ProductImageError";

        private readonly IProductImageService _productImageService;
        private readonly IProductService _productService;

        public ProductImageController(IProductImageService productImageService, IProductService productService)
        {
            _productImageService = productImageService;
            _productService = productService;
        }

        [Route("ProductImageDetail/{id}")]
        [HttpGet]
        public async Task<IActionResult> ProductImageDetail(string id)
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Görselleri";
            ViewBag.v0 = "Ürün İşlemleri";

            var product = await _productService.GetByIdProductAsync(id);
            ViewBag.ProductId = id;
            ViewBag.ProductName = product?.ProductName;

            var images = await _productImageService.GetByProductIdProductImageAsync(id);
            return View(images);
        }

        [Route("Upload")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string productId, IFormFile? file, bool isMain)
        {
            if (file is null || file.Length == 0)
                TempData[ErrorKey] = "Lütfen bir görsel dosyası seçin.";
            else if (await _productImageService.UploadProductImageAsync(productId, file, isMain) is string error)
                TempData[ErrorKey] = error;

            return RedirectToDetail(productId);
        }

        [Route("SetMain/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMain(string id, string productId)
        {
            if (await _productImageService.SetMainProductImageAsync(id) is string error)
                TempData[ErrorKey] = error;
            return RedirectToDetail(productId);
        }

        [Route("Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, string productId)
        {
            if (await _productImageService.DeleteProductImageAsync(id) is string error)
                TempData[ErrorKey] = error;
            return RedirectToDetail(productId);
        }

        private IActionResult RedirectToDetail(string productId)
            => RedirectToAction(nameof(ProductImageDetail), new { area = "Admin", id = productId });
    }
}
