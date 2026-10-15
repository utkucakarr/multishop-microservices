using MassTransit;
using MultiShop.Basket.Clients;
using MultiShop.Basket.Data;
using MultiShop.Basket.Dtos;
using MultiShop.Basket.Models;
using MultiShop.Basket.Pricing;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Contracts.Baskets;

namespace MultiShop.Basket.Services
{
    public class BasketService : IBasketService
    {
        // Çift tıklamayı kapsayacak kadar uzun, sipariş açılamazsa (hata) kilit hemen bırakılır.
        private static readonly TimeSpan CheckoutLockDuration = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(10);

        private readonly BasketRepository _repository;
        private readonly CatalogClient _catalogClient;
        private readonly DiscountClient _discountClient;
        private readonly PricingCalculator _pricingCalculator;
        private readonly OrderClient _orderClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<BasketService> _logger;

        public BasketService(BasketRepository repository, CatalogClient catalogClient, DiscountClient discountClient, PricingCalculator pricingCalculator,
            OrderClient orderClient, IPublishEndpoint publishEndpoint, ILogger<BasketService> logger)
        {
            _orderClient = orderClient;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _repository = repository;
            _catalogClient = catalogClient;
            _discountClient = discountClient;
            _pricingCalculator = pricingCalculator;
        }

        public async Task<BasketDto> GetBasketAsync(string userId)
        {
            var basket = await _repository.GetAsync(userId);
            var warnings = new List<string>();
            var changed = false;

            // Fiyat, ad ve görsel her okumada Catalog'dan (tek istek). Catalog'da artık olmayan ürün sepetten çıkar.
            var products = await _catalogClient.GetPricesAsync(basket.Items.Select(x => x.ProductId).ToList());
            var removed = basket.Items.RemoveAll(x => !products.ContainsKey(x.ProductId));
            if (removed > 0)
            {
                warnings.Add(removed == 1
                    ? "Sepetinizdeki bir ürün artık satışta olmadığı için çıkarıldı."
                    : $"Sepetinizdeki {removed} ürün artık satışta olmadığı için çıkarıldı.");
                changed = true;
            }

            // Kupon uygulandıktan sonra pasifleşmiş ya da süresi dolmuş olabilir: her okumada yeniden doğrulanır.
            var discountRate = 0;
            if (basket.CouponCode is not null)
            {
                var coupon = await _discountClient.ValidateAsync(basket.CouponCode);
                if (coupon is null)
                {
                    warnings.Add($"'{basket.CouponCode}' kuponu artık geçerli olmadığı için kaldırıldı.");
                    basket.CouponCode = null;
                    changed = true;
                }
                else
                {
                    discountRate = coupon.Rate;
                }
            }

            if (changed)
                await _repository.SaveAsync(userId, basket);

            var lines = basket.Items.Select(item =>
            {
                var product = products[item.ProductId];
                var line = new PricingLine(product.ProductPrice, item.Quantity);
                return (Product: product, Line: line);
            }).ToList();

            var totals = _pricingCalculator.Calculate(lines.Select(x => x.Line).ToList(), discountRate);

            return new BasketDto
            {
                Items = lines.Select(x => new BasketLineDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName,
                    ProductImageUrl = x.Product.ProductImageUrl,
                    UnitPrice = x.Line.UnitPrice,
                    Quantity = x.Line.Quantity,
                    LineTotal = PricingCalculator.LineTotal(x.Line),
                }).ToList(),
                CouponCode = basket.CouponCode,
                DiscountRate = totals.DiscountRate,
                Subtotal = totals.Subtotal,
                DiscountAmount = totals.DiscountAmount,
                TaxRatePercent = totals.TaxRatePercent,
                TaxAmount = totals.TaxAmount,
                ShippingFee = totals.ShippingFee,
                GrandTotal = totals.GrandTotal,
                Warnings = warnings,
            };
        }

        public async Task<int> GetItemCountAsync(string userId)
            => (await _repository.GetAsync(userId)).Items.Count;

        public async Task AddItemAsync(string userId, AddBasketItemDto item)
        {
            var products = await _catalogClient.GetPricesAsync(new[] { item.ProductId });
            if (!products.ContainsKey(item.ProductId))
                throw new NotFoundException("Ürün", item.ProductId);

            var basket = await _repository.GetAsync(userId);
            var existing = basket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            var newQuantity = (existing?.Quantity ?? 0) + item.Quantity;
            EnsureQuantityInRange(newQuantity);

            if (existing is null)
                basket.Items.Add(new StoredBasketItem { ProductId = item.ProductId, Quantity = item.Quantity });
            else
                existing.Quantity = newQuantity;

            await _repository.SaveAsync(userId, basket);
        }

        public async Task SetQuantityAsync(string userId, string productId, int quantity)
        {
            EnsureQuantityInRange(quantity);

            var basket = await _repository.GetAsync(userId);
            var existing = basket.Items.FirstOrDefault(x => x.ProductId == productId)
                ?? throw new NotFoundException("Sepetteki ürün", productId);
            existing.Quantity = quantity;

            await _repository.SaveAsync(userId, basket);
        }

        public async Task RemoveItemAsync(string userId, string productId)
        {
            var basket = await _repository.GetAsync(userId);
            if (basket.Items.RemoveAll(x => x.ProductId == productId) > 0)
                await _repository.SaveAsync(userId, basket);
        }

        public async Task ApplyCouponAsync(string userId, string code)
        {
            var coupon = await _discountClient.ValidateAsync(code.Trim())
                ?? throw new BadRequestException("Kupon geçersiz veya süresi dolmuş.");

            var basket = await _repository.GetAsync(userId);
            basket.CouponCode = coupon.Code;
            await _repository.SaveAsync(userId, basket);
        }

        public async Task RemoveCouponAsync(string userId)
        {
            var basket = await _repository.GetAsync(userId);
            basket.CouponCode = null;
            await _repository.SaveAsync(userId, basket);
        }

        public Task DeleteBasketAsync(string userId) => _repository.DeleteAsync(userId);

        public async Task<CheckoutResultDto> CheckoutAsync(string userId, CheckoutDto request)
        {
            var lockToken = Guid.NewGuid().ToString("N");
            if (!await _repository.TryAcquireCheckoutLockAsync(userId, lockToken, CheckoutLockDuration))
                throw new ConflictException("Siparişiniz oluşturuluyor, lütfen bekleyin.");

            try
            {
                // Onay anındaki fiyat ve kupon: sepet Catalog ve Discount'tan yeniden hesaplanır.
                var basket = await GetBasketAsync(userId);

                if (basket.Items.Count == 0)
                    throw new BadRequestException("Sepetiniz boş.");

                // Okurken sepet değişti (ürün satıştan kalktı, kupon geçersizleşti): kullanıcı yeni sepeti görmeden sipariş açılmaz.
                if (basket.Warnings.Count > 0)
                    throw new ConflictException(string.Join(" ", basket.Warnings) + " Lütfen sepetinizi kontrol edip tekrar onaylayın.");

                if (basket.GrandTotal != request.ExpectedTotal)
                    throw new ConflictException("Sepet tutarı değişti. Lütfen güncel tutarı kontrol edip tekrar onaylayın.");

                if (!await _orderClient.IsOwnAddressAsync(request.AddressId))
                    throw new BadRequestException("Teslimat adresi bulunamadı.");

                var orderNumber = Guid.NewGuid();
                await PublishCheckedOutAsync(new BasketCheckedOut(
                    orderNumber,
                    userId,
                    request.AddressId,
                    basket.Items.Select(x => new BasketCheckedOutItem(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity, x.LineTotal)).ToList(),
                    basket.CouponCode,
                    basket.DiscountRate,
                    basket.Subtotal,
                    basket.DiscountAmount,
                    basket.TaxAmount,
                    basket.ShippingFee,
                    basket.GrandTotal,
                    DateTime.UtcNow));

                _logger.LogInformation("Sepet onaylandı: sipariş {OrderNumber}, {Total} TL.", orderNumber, basket.GrandTotal);
                return new CheckoutResultDto { OrderNumber = orderNumber };
            }
            catch
            {
                // Sipariş açılmadı: kullanıcı beklemeden tekrar deneyebilsin. Başarıda kilit süresi dolana kadar kalır (çift tıklama).
                await _repository.ReleaseCheckoutLockAsync(userId, lockToken);
                throw;
            }
        }

        // Redis'te transaction olmadığı için outbox yok: event doğrudan yayınlanır. RabbitMQ'ya ulaşılamazsa istek 503 alır
        // ve sipariş hiç oluşmaz (yarım kalan bir şey yok); kullanıcı tekrar dener.
        private async Task PublishCheckedOutAsync(BasketCheckedOut message)
        {
            using var timeout = new CancellationTokenSource(PublishTimeout);
            try
            {
                await _publishEndpoint.Publish(message, timeout.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BasketCheckedOut yayınlanamadı (sipariş {OrderNumber}).", message.OrderNumber);
                throw new ServiceUnavailableException("Siparişiniz şu anda alınamıyor, lütfen biraz sonra tekrar deneyin.", ex);
            }
        }

        private static void EnsureQuantityInRange(int quantity)
        {
            if (quantity is < BasketLimits.MinQuantity or > BasketLimits.MaxQuantity)
                throw new BadRequestException($"Bir üründen en az {BasketLimits.MinQuantity}, en fazla {BasketLimits.MaxQuantity} adet alınabilir.");
        }
    }
}
