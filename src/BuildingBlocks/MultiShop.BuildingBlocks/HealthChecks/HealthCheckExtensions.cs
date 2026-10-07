using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MultiShop.BuildingBlocks.HealthChecks;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>
    /// <c>/health</c>: servis ve bağımlılıkları (veritabanı vb.) – hepsi sağlıklıysa 200, değilse 503.<br/>
    /// <c>/health/live</c>: yalnızca sürecin ayakta olduğu, bağımlılık kontrolü yapılmaz.
    /// </summary>
    /// <remarks>
    /// Endpoint yerine middleware olarak eklenir; böylece kimlik doğrulamadan ve
    /// tüm istekleri yakalayan Ocelot'tan önce çalışır.
    /// </remarks>
    public static IApplicationBuilder UseMultiShopHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponseAsync,
        });

        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
        });

        return app;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description ?? entry.Value.Exception?.Message,
                }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
