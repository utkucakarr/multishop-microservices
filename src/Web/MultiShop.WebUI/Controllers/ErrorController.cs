using Microsoft.AspNetCore.Mvc;

namespace MultiShop.WebUI.Controllers
{
    // Hata sayfaları. Servis çağırmazlar ve kendi küçük layout'larını kullanırlar
    // (site layout'undaki navbar Catalog'u çağırır; Catalog kapalıyken hata sayfası da çökmesin).
    // HTTP metodu kısıtlanmaz: hata bir POST sırasında da oluşabilir ve istek aynı metotla yeniden çalıştırılır.
    [Route("Error")]
    public class ErrorController : Controller
    {
        // UseExceptionHandler (Development dışı): beklenmeyen kod hataları.
        [Route("")]
        public IActionResult Index()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            ViewBag.Title = "Beklenmeyen bir hata oluştu";
            ViewBag.Message = "İsteğiniz işlenirken bir sorun oluştu. Lütfen biraz sonra tekrar deneyin.";
            return View();
        }

        // ServiceExceptionMiddleware: servis 5xx döndü ya da hiç ulaşılamadı.
        [Route("ServiceUnavailable")]
        public IActionResult ServiceUnavailable()
        {
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ViewBag.Title = "Servis şu anda yanıt vermiyor";
            ViewBag.Message = "Lütfen biraz sonra tekrar deneyin.";
            return View("Index");
        }

        // UseStatusCodePagesWithReExecute: gövdesi boş hata yanıtları. Yalnızca 404 için sayfa gösterilir;
        // diğer kodlar (ör. fetch ile çağrılan uçların 401'i) gövdesiz olarak aynen döner.
        [Route("{code:int}")]
        public IActionResult Status(int code)
        {
            if (code != StatusCodes.Status404NotFound)
                return StatusCode(code);

            Response.StatusCode = StatusCodes.Status404NotFound;
            ViewBag.Title = "Sayfa bulunamadı";
            ViewBag.Message = "Aradığınız sayfa ya da kayıt bulunamadı. Silinmiş ya da adresi değişmiş olabilir.";
            return View("Index");
        }
    }
}
