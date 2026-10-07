using Microsoft.Extensions.Diagnostics.HealthChecks;
using MultiShop.Basket.Settings;

namespace MultiShop.Basket.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public RedisHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // RedisService ilk kez oluşturulurken bağlanır ve Redis kapalıysa exception fırlatır;
                // bu yüzden constructor'da değil, try bloğunun içinde alınıyor.
                var redisService = _serviceProvider.GetRequiredService<RedisService>();
                var latency = await redisService.GetDb().PingAsync();
                return HealthCheckResult.Healthy($"Ping {latency.TotalMilliseconds:0.#} ms");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis'e ulaşılamıyor.", ex);
            }
        }
    }
}
