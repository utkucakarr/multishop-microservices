using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Basket.Clients
{
    /// <summary>Catalog'daki güncel ürün bilgisi (<c>GET api/products/by-ids</c> yanıtı).</summary>
    public record CatalogProductPrice(string ProductId, string ProductName, decimal ProductPrice, string? ProductImageUrl);

    public class CatalogClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogClient> _logger;

        public CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>Bulunan ürünler id'ye göre; Catalog'da olmayan ürünler sözlükte yer almaz.</summary>
        public async Task<IReadOnlyDictionary<string, CatalogProductPrice>> GetPricesAsync(IReadOnlyCollection<string> productIds, CancellationToken cancellationToken = default)
        {
            if (productIds.Count == 0)
                return new Dictionary<string, CatalogProductPrice>();

            var query = string.Join("&", productIds.Select(id => "ids=" + Uri.EscapeDataString(id)));
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<CatalogProductPrice>>("api/products/by-ids?" + query, cancellationToken)
                    ?? new List<CatalogProductPrice>();
                return products.ToDictionary(x => x.ProductId);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Catalog'dan ürün fiyatları alınamadı.");
                throw new ServiceUnavailableException("Ürün bilgileri şu anda alınamıyor, lütfen biraz sonra tekrar deneyin.", ex);
            }
        }
    }
}
