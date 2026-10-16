using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MultiShop.WebUI.Services;
using System.Net;

namespace MultiShop.WebUI.Middlewares
{
    /// <summary>
    /// Sayfa üretilirken (controller ya da view component) servis çağrısı başarısız olursa kullanıcıyı uygun sayfaya götürür:
    /// 401 → oturum kapatılıp girişe, 403 → erişim reddedildi, 404 → bulunamadı, diğerleri → servis yanıt vermiyor.
    /// Yalnızca servis hatalarını ele alır; kod hataları geliştirici sayfasına / UseExceptionHandler'a düşer.
    /// UseRouting'den önce kayıtlı olmalı: hata sayfası, isteğin yolu değiştirilip pipeline yeniden çalıştırılarak üretilir.
    /// </summary>
    public class ServiceExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ServiceExceptionMiddleware> _logger;

        public ServiceExceptionMiddleware(RequestDelegate next, ILogger<ServiceExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ServiceApiException ex) when (!context.Response.HasStarted)
            {
                await HandleServiceErrorAsync(context, ex);
            }
            catch (HttpRequestException ex) when (!context.Response.HasStarted && ex.StatusCode is not null)
            {
                // GetFromJsonAsync başarısız yanıtta durum kodlu HttpRequestException fırlatır.
                await HandleServiceErrorAsync(context, new ServiceApiException(ex.StatusCode.Value, null, ex.Message));
            }
            catch (HttpRequestException ex) when (!context.Response.HasStarted)
            {
                // Gateway'e (ya da IdentityServer'a) hiç ulaşılamadı.
                _logger.LogError(ex, "Servise ulaşılamadı: {Method} {Path}", context.Request.Method, context.Request.Path);
                await ReExecuteAsync(context, "/Error/ServiceUnavailable");
            }
        }

        private async Task HandleServiceErrorAsync(HttpContext context, ServiceApiException ex)
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

            switch (ex.StatusCode)
            {
                // Token yenilense bile reddedildi (ör. refresh token'ın süresi doldu): yeniden giriş gerekir.
                case HttpStatusCode.Unauthorized when isAuthenticated:
                    _logger.LogWarning("Servis 401 döndü, oturum kapatılıyor: {RequestUri} ({Path})", ex.RequestUri, context.Request.Path);
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    var returnUrl = HttpMethods.IsGet(context.Request.Method)
                        ? context.Request.PathBase + context.Request.Path + context.Request.QueryString
                        : "/";
                    context.Response.Redirect("/Login/Index?returnUrl=" + Uri.EscapeDataString(returnUrl));
                    return;

                case HttpStatusCode.Forbidden:
                    _logger.LogWarning("Servis 403 döndü: {RequestUri} ({Path})", ex.RequestUri, context.Request.Path);
                    context.Response.Redirect("/Login/AccessDenied");
                    return;

                case HttpStatusCode.NotFound:
                    _logger.LogWarning("Servis 404 döndü: {RequestUri} ({Path}) {Message}", ex.RequestUri, context.Request.Path, ex.Message);
                    await ReExecuteAsync(context, "/Error/404");
                    return;

                default:
                    // Ziyaretçi token'ının reddedilmesi (401, giriş yapılmamış) de kullanıcının düzeltebileceği bir durum değil.
                    _logger.LogError(ex, "Servis hatası {StatusCode}: {RequestUri} ({Path})", (int)ex.StatusCode, ex.RequestUri, context.Request.Path);
                    await ReExecuteAsync(context, "/Error/ServiceUnavailable");
                    return;
            }
        }

        // UseExceptionHandler'ın yaptığı gibi: yanıtı temizleyip hata sayfasının yolunu pipeline'da yeniden çalıştırır.
        private async Task ReExecuteAsync(HttpContext context, string path)
        {
            var originalPath = context.Request.Path;
            // Bu istekte token yenilendiyse yeni token'lar cookie'de; yanıt temizlenirken kaybolmasın
            // (refresh token tek kullanımlık, eskisiyle bir sonraki yenileme başarısız olur).
            var setCookie = context.Response.Headers.SetCookie;
            context.Response.Clear();
            context.Response.Headers.SetCookie = setCookie;
            context.SetEndpoint(null);
            context.Request.RouteValues.Clear();
            context.Request.Path = path;
            try
            {
                await _next(context);
            }
            finally
            {
                context.Request.Path = originalPath;
            }
        }
    }
}
