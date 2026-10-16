using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Context;
using Testcontainers.MsSql;

namespace MultiShop.Payment.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Testler boyunca tek bir SQL Server container'ı (docker-compose ile aynı imaj) ve Payment migration'ları uygulanmış
    /// tek bir veritabanı: koşullu UPDATE'ler (ExecuteUpdate) gerçek SQL Server'da denenir. Her test kendi sipariş
    /// numarasıyla çalışır; birbirinin kayıtlarına bakmaz.
    /// </summary>
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        public DbContextOptions<PaymentContext> Options { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            Options = await CreateDatabaseAsync();
        }

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        private async Task<DbContextOptions<PaymentContext>> CreateDatabaseAsync()
        {
            var connectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = $"payment_{Guid.NewGuid():N}"
            }.ConnectionString;

            var options = new DbContextOptionsBuilder<PaymentContext>().UseSqlServer(connectionString).Options;
            await using var context = new PaymentContext(options);
            await context.Database.MigrateAsync();
            return options;
        }
    }

    [CollectionDefinition(Name)]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
        public const string Name = "SqlServer";
    }
}
