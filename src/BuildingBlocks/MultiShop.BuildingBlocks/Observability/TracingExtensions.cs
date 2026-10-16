using MassTransit.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MultiShop.BuildingBlocks.Observability;

public static class TracingExtensions
{
    // Seq'in OTLP girişi (yalnızca http/protobuf); arayüzde Traces/Spans olarak görünür.
    private const string DefaultOtlpEndpoint = "http://localhost:8091/ingest/otlp/v1/traces";

    // WebUI'nin statik dosyaları her sayfada onlarca span üretip Seq'i dolduruyordu.
    private static readonly HashSet<string> StaticFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".map", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp", ".ico",
        ".woff", ".woff2", ".ttf", ".eot"
    };

    /// <summary>
    /// OpenTelemetry tracing: gelen HTTP istekleri, giden HttpClient çağrıları ve MassTransit
    /// (RabbitMQ publish/consume, saga) span'leri. MassTransit trace bağlamını mesaj başlığında taşıdığı için
    /// bir checkout'un WebUI → Gateway → Order → Payment → … yolculuğu Seq'te tek bir trace olarak görünür.
    /// Serilog log'lara aktif TraceId/SpanId'yi kendisi eklediğinden loglar da aynı trace'e bağlanır.
    /// Adres <c>OpenTelemetry:OtlpEndpoint</c> ile değiştirilebilir.
    /// </summary>
    /// <param name="serviceName">Seq'teki <c>service.name</c>; loglardaki <c>Application</c> ile aynı, ör. <c>Order</c>.</param>
    public static IServiceCollection AddMultiShopTracing(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        var endpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? DefaultOtlpEndpoint;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.Filter = context => !IsIgnoredRequest(context.Request.Path))
                // Serilog'un Seq'e log gönderdiği istekler (/ingest/clef) span'e dönüşmesin.
                .AddHttpClientInstrumentation(options => options.FilterHttpRequestMessage = request =>
                    request.RequestUri?.AbsolutePath.StartsWith("/ingest/", StringComparison.OrdinalIgnoreCase) != true)
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                }));

        return services;
    }

    private static bool IsIgnoredRequest(PathString path)
        => path.StartsWithSegments("/health")
           || path.StartsWithSegments("/swagger")
           || StaticFileExtensions.Contains(Path.GetExtension(path.Value ?? string.Empty));
}
