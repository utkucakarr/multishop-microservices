using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Basket.Clients
{
    /// <summary>Order'daki adres (<c>GET api/addresses/mine</c> yanıtından yalnızca id).</summary>
    public record AddressSummary(int AddressId);

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
        /// Adres token'daki kullanıcının kendi adreslerinden biri mi? <c>api/addresses/{id}</c> yerine <c>mine</c> listesine
        /// bakılır: Admin her adrese erişebildiği için tekil uç nokta Admin'e başkasının adresini de "senin" der.
        /// </summary>
        public async Task<bool> IsOwnAddressAsync(int addressId, CancellationToken cancellationToken = default)
        {
            try
            {
                var addresses = await _httpClient.GetFromJsonAsync<List<AddressSummary>>("api/addresses/mine", cancellationToken)
                    ?? new List<AddressSummary>();
                return addresses.Any(x => x.AddressId == addressId);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Order'dan adres doğrulanamadı.");
                throw new ServiceUnavailableException("Teslimat adresi şu anda doğrulanamıyor, lütfen biraz sonra tekrar deneyin.", ex);
            }
        }
    }
}
