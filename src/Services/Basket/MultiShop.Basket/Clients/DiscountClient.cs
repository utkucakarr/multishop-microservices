using System.Net;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Basket.Clients
{
    /// <summary>Geçerli kuponun bilgisi (<c>GET api/discounts/validate/{code}</c> yanıtı).</summary>
    public record CouponValidation(string Code, int Rate);

    public class DiscountClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiscountClient> _logger;

        public DiscountClient(HttpClient httpClient, ILogger<DiscountClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>Kupon aktif ve süresi dolmamışsa bilgisi; değilse (Discount 404) null.</summary>
        public async Task<CouponValidation?> ValidateAsync(string code, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync("api/discounts/validate/" + Uri.EscapeDataString(code), cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<CouponValidation>(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Discount'tan kupon doğrulanamadı.");
                throw new ServiceUnavailableException("Kupon şu anda doğrulanamıyor, lütfen biraz sonra tekrar deneyin.", ex);
            }
        }
    }
}
