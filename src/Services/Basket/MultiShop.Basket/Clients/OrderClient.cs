using System.Net;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Basket.Clients
{
    public class OrderClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderClient> _logger;

        public OrderClient(HttpClient httpClient, ILogger<OrderClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Adres token'daki kullanıcıya mı ait? Order başkasının adresine de "yok" (404) der (IDOR, G-14),
        /// bu yüzden 200 = kullanıcının adresi, 404 = yok ya da başkasının.
        /// </summary>
        public async Task<bool> IsOwnAddressAsync(int addressId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync("api/addresses/" + addressId, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Order'dan adres doğrulanamadı.");
                throw new ServiceUnavailableException("Teslimat adresi şu anda doğrulanamıyor, lütfen biraz sonra tekrar deneyin.", ex);
            }
        }
    }
}
