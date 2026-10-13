using Microsoft.Net.Http.Headers;

namespace MultiShop.Basket.Clients
{
    /// <summary>
    /// Token relay: Basket, Catalog ve Discount'a sepetin sahibi kullanıcının token'ıyla gider. Kullanıcının token'ında
    /// bu servislerin scope'ları zaten var; ayrı bir servis kimliği (client credentials) kullanıcısız çağrılar gelince (KT-12) eklenir.
    /// </summary>
    public class ForwardAccessTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ForwardAccessTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authorization = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            if (!string.IsNullOrEmpty(authorization))
                request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, authorization);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
