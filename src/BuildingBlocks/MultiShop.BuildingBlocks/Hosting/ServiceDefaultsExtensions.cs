using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.BuildingBlocks.HealthChecks;
using MultiShop.BuildingBlocks.Logging;

namespace MultiShop.BuildingBlocks.Hosting;

public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Her mikroservisin ortak kurulumu: user-secrets önceliği, Serilog (konsol + Seq),
    /// ProblemDetails ile hata yönetimi, health check'ler ve <see cref="ICurrentUserService"/>.
    /// Bağlantı dizeleri okunmadan önce, <c>CreateBuilder</c>'dan hemen sonra çağrılmalı.
    /// </summary>
    /// <param name="serviceName">Loglardaki <c>Application</c> değeri, ör. <c>Catalog</c>.</param>
    public static WebApplicationBuilder AddMultiShopServiceDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        builder.AddUserSecretsOverEnvironment();
        builder.Host.UseMultiShopSerilog(serviceName);

        builder.Services.AddProblemDetails(options =>
            // Yanıttaki traceId ile Seq'te aynı isteğin loglarına ulaşılabilir.
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier));
        builder.Services.AddOptions<ExceptionMappingOptions>();
        builder.Services.AddHealthChecks();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        return builder;
    }

    /// <summary>
    /// Hata middleware'i, <c>/health</c>, istek logları ve (Development'ta) Swagger.
    /// <c>UseAuthentication</c>'dan önce çağrılmalı.
    /// </summary>
    public static WebApplication UseMultiShopServiceDefaults(this WebApplication app)
    {
        // Sıra önemli: health check istekleri loglanmaz; istek logu hata middleware'inin dışında kalır,
        // böylece exception'ın kendisini değil, istemciye dönen son durum kodunu (ör. 404) loglar.
        app.UseMultiShopHealthChecks();
        app.UseMultiShopRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // Development'ta servisler yalnızca http dinliyor; yönlendirme "https port" uyarısı veriyordu.
            app.UseHttpsRedirection();
        }

        return app;
    }

    /// <summary>
    /// User-secrets'ı ortam değişkenlerinden sonra tekrar ekler. Böylece başka bir yerel projeden kalan
    /// makine/kullanıcı düzeyindeki ortam değişkenleri (ör. <c>ConnectionStrings__DefaultConnection</c>)
    /// bu projenin user-secrets değerlerini ezemez.
    /// </summary>
    private static void AddUserSecretsOverEnvironment(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
            return;

        // GetEntryAssembly() EF araçlarıyla (Update-Database) çalışırken ef.dll'i döndürür; uygulama adı her zaman doğru.
        var appAssembly = Assembly.Load(new AssemblyName(builder.Environment.ApplicationName));
        builder.Configuration.AddUserSecrets(appAssembly, optional: true);
    }
}
