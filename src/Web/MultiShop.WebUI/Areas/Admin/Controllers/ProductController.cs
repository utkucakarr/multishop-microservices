using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public ProductController(IProductService productService, ICategoryService categoryService, IBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }
        void ProductViewBagList()
        {
            ViewBag.v1 = "Anasayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Listesi";
            ViewBag.v0 = "Ürün İşemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ProductViewBagList();
            var values = await _productService.GetAllProductAsync();
            // Liste ürün başına marka adı göstersin diye: markalar tek istekte alınıp id → ad eşlenir.
            var brands = await _brandService.GetAllBrandAsync();
            ViewBag.BrandNames = brands.ToDictionary(x => x.BrandId, x => x.BrandName);
            return View(values);
        }

        [HttpPost]
        [Route("UpdateStock/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(string id, int availableStock)
        {
            var error = await _productService.UpdateStockAsync(id, availableStock);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Stok güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct()
        {
            ProductViewBagList();
            await LoadSelectListsAsync();
            return View(new CreateProductDto());
        }

        [HttpPost]
        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            var error = await _productService.CreateProductAsync(createProductDto);
            if (error is null)
                return RedirectToAction("Index", "Product", new { area = "Admin" });

            // Catalog reddettiyse (ör. stok sınırları, olmayan kategori/marka) form girilen değerlerle ve mesajla geri gelir.
            ViewBag.ErrorMessage = error;
            ProductViewBagList();
            await LoadSelectListsAsync();
            return View(createProductDto);
        }

        [Route("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var error = await _productService.DeleteProductAsync(id);
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Ürün silindi.";
            return RedirectToAction("Index", "Product", new { area = "Admin" });
        }

        [Route("UpdateProduct/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string id)
        {
            ProductViewBagList();
            await LoadSelectListsAsync();

            var product = await _productService.GetByIdProductAsync(id);
            return View(new UpdateProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductPrice = product.ProductPrice,
                // ProductImageUrl bilerek boş: görseller "Görseller" sayfasından yönetilir; Catalog boş gelen değerde mevcut değeri korur.
                ProductDescription = product.ProductDescription,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId
            });
        }

        [Route("UpdateProduct/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
            var error = await _productService.UpdateProductAsync(updateProductDto);
            if (error is null)
                return RedirectToAction("Index", "Product", new { area = "Admin" });

            ViewBag.ErrorMessage = error;
            ProductViewBagList();
            await LoadSelectListsAsync();
            return View(updateProductDto);
        }

        // Kategori (zorunlu) ve marka (isteğe bağlı, "Marka yok" seçeneğiyle) açılır listeleri.
        private async Task LoadSelectListsAsync()
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            ViewBag.CategoryValues = categories
                .Select(x => new SelectListItem { Text = x.CategoryName, Value = x.CategoryId })
                .ToList();

            var brands = await _brandService.GetAllBrandAsync();
            ViewBag.BrandValues = brands
                .OrderBy(x => x.BrandName)
                .Select(x => new SelectListItem { Text = x.BrandName, Value = x.BrandId })
                .ToList();
        }
    }
}
