using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiShop.Catalog.Controllers;
using Testcontainers.MongoDb;

namespace MultiShop.Catalog.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Catalog API'si bellekte (TestServer), gerçek MongoDB ile (Testcontainers, docker-compose ile aynı imaj).
    /// Mongo tek düğümlü replica set: stok komutları MassTransit Mongo outbox'ının transaction'ında çalışır.
    /// RabbitMQ yerine MassTransit test harness'ı, IdentityServer token'ı yerine <see cref="TestAuthHandler"/>.
    /// "Testing" ortamı: Development'taki gibi user-secrets yüklenmez (testler geliştirme veritabanına bağlanamaz).
    /// </summary>
    public sealed class CatalogApiFactory : WebApplicationFactory<CategoriesController>, IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();

        public Task InitializeAsync() => _mongo.StartAsync();

        async Task IAsyncLifetime.DisposeAsync()
        {
            await DisposeAsync();
            await _mongo.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataBaseSettings:ConnectionString"] = _mongo.GetConnectionString(),
                ["DataBaseSettings:DatabaseName"] = $"catalog_tests_{Guid.NewGuid():N}",
                // Bus test harness'ıyla değiştirildiği için kullanılmaz; yalnızca ayar doğrulaması için.
                ["ConnectionStrings:RabbitMQ"] = "amqp://guest:guest@localhost:5672",
                // Test logları geliştirme Seq'ine gitmesin.
                ["Seq:ServerUrl"] = "http://localhost:1",
            }));

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
                services.AddMassTransitTestHarness(x => x.AddConsumer<StockEventProbe>());
            });
        }

        public HttpClient CreateClientAs(TestUser user)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, user.ToString());
            return client;
        }
    }

    [CollectionDefinition(Name)]
    public class CatalogApiCollection : ICollectionFixture<CatalogApiFactory>
    {
        public const string Name = "CatalogApi";
    }
}
