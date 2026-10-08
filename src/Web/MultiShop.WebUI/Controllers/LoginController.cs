using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MultiShop.WebUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIdentityService _identityService;

        public LoginController(IHttpClientFactory httpClientFactory, IIdentityService identityService)
        {
            _httpClientFactory = httpClientFactory;
            _identityService = identityService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(SignInDto signInDto, string returnUrl)
        {
            var isSignedIn = await _identityService.SignIn(signInDto);
            if (!isSignedIn)
            {
                ViewBag.LoginError = "Kullanıcı adı veya şifre hatalı ya da kimlik sunucusuna ulaşılamadı.";
                return View();
            }
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Default");
            }
            return Redirect(returnUrl);
        }

        // Giriş yapmış ama yetkisi olmayan kullanıcı buraya yönlenir (Program.cs → AccessDeniedPath).
        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _identityService.Logout();
            return RedirectToAction("Index", "Default");
        }
    }
}