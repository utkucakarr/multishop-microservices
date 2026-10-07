using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.HealthChecks
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IDatabaseSettings _databaseSettings;

        public MongoHealthCheck(IDatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // MongoClient aynı bağlantı dizesi için alttaki bağlantı havuzunu paylaşır; her kontrolde yeni bağlantı açılmaz.
                var database = new MongoClient(_databaseSettings.ConnectionString).GetDatabase(_databaseSettings.DatabaseName);
                await database.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB'ye ulaşılamıyor.", ex);
            }
        }
    }
}
