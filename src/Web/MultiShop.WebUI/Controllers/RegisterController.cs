using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;
using Newtonsoft.Json;
using System.Text;

namespace MultiShop.WebUI.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateRegisterDto createRegisterDto)
        {
            if (createRegisterDto.Password != createRegisterDto.ConfirmPassword)
            {
                ViewBag.Errors = new[] { "Şifreler eşleşmiyor." };
                return View();
            }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createRegisterDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("http://localhost:5010/api/Registers", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Login");
            }

            // IdentityServer hata nedenlerini 400 ile döner (ör. şifre kuralları, kullanılan kullanıcı adı).
            var errors = responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest
                ? ReadErrors(await responseMessage.Content.ReadAsStringAsync())
                : null;
            ViewBag.Errors = errors is { Length: > 0 } ? errors : new[] { "Kayıt sırasında bir hata oluştu, tekrar deneyin." };
            return View();
        }

        private static string[]? ReadErrors(string body)
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(body, new { errors = Array.Empty<string>() })?.errors;
            }
            catch (JsonException)
            {
                // Model doğrulama hatası gibi farklı biçimdeki yanıtlar için genel mesaj gösterilir.
                return null;
            }
        }
    }
}
