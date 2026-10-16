using System.Net;

namespace MultiShop.WebUI.Services
{
    /// <summary>
    /// Bir servisten (gateway ya da IdentityServer) başarısız yanıt geldi. Sayfa üretilirken fırlatılır;
    /// <see cref="Middlewares.ServiceExceptionMiddleware"/> durum koduna göre kullanıcıyı uygun sayfaya yönlendirir.
    /// </summary>
    public class ServiceApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public Uri? RequestUri { get; }

        public ServiceApiException(HttpStatusCode statusCode, Uri? requestUri, string? message)
            : base(message ?? $"Servis isteği başarısız oldu ({(int)statusCode}).")
        {
            StatusCode = statusCode;
            RequestUri = requestUri;
        }
    }
}
