using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MultiShop.Catalog.HealthChecks
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoDatabase _database;

        public MongoHealthCheck(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB'ye ulaşılamıyor.", ex);
            }
        }
    }
}
