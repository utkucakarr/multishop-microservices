using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MultiShop.BuildingBlocks.Logging;

public static class SerilogExtensions
{
    private const string DefaultSeqUrl = "http://localhost:8091";

    /// <summary>
    /// Logları konsola ve Seq'e yazar. Her log kaydına <c>Application</c> özelliği eklenir,
    /// böylece Seq'te servis bazında filtrelenebilir (ör. <c>Application = 'Catalog'</c>).
    /// Seviyeler appsettings'teki <c>Serilog</c> bölümüyle ezilebilir; Seq adresi <c>Seq:ServerUrl</c>.
    /// </summary>
    public static IHostBuilder UseMultiShopSerilog(this IHostBuilder hostBuilder, string applicationName)
        => hostBuilder.UseSerilog((context, services, logger) => logger
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? DefaultSeqUrl));
}
