using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;
using MultiShop.WebUI.Services.UserIdentityServices;

namespace MultiShop.WebUI.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
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

            var errors = await _registerService.RegisterAsync(createRegisterDto);
            if (errors is null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Errors = errors;
            return View();
        }
    }
}
