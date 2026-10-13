using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MultiShop.BuildingBlocks.Messaging;

public static class MessagingExtensions
{
    /// <summary>RabbitMQ bağlantısının adı: <c>ConnectionStrings:RabbitMQ</c> = <c>amqp://kullanici:sifre@localhost:5673</c>.</summary>
    public const string ConnectionStringName = "RabbitMQ";

    /// <summary>
    /// MassTransit + RabbitMQ ortak kurulumu: bağlantı, kuyruk adları, retry ve health check.
    /// Outbox/inbox ve consumer'lar servise özgü olduğu için <paramref name="configure"/> içinde eklenir.
    /// </summary>
    /// <param name="serviceName">Kuyruk adlarının ön eki: <c>Notification</c> + <c>CommentCreatedConsumer</c> → <c>notification-comment-created</c>.</param>
    /// <param name="configure">Servisin consumer'ları ve EF outbox ayarı.</param>
    public static WebApplicationBuilder AddMultiShopMessaging(this WebApplicationBuilder builder, string serviceName,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(serviceName, includeNamespace: false));

            // Broker kapalıyken servis çalışmaya devam eder: gönderilecek event'ler outbox'ta bekler.
            // Bu yüzden /health 503 (Unhealthy) değil, Degraded döner.
            x.ConfigureHealthCheckOptions(options => options.MinimalFailureStatus = HealthStatus.Degraded);

            // Retry, outbox/inbox'tan ÖNCE eklenir (callback'ler kayıt sırasıyla çalışır): her deneme yeni bir transaction'dır.
            // Üç denemede de hata alan mesaj <kuyruk>_error kuyruğuna taşınır; düzeltilince panelden geri gönderilebilir.
            x.AddConfigureEndpointsCallback((_, _, endpoint) =>
                endpoint.UseMessageRetry(retry => retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15))));

            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                // Bağlantı bus başlarken okunur, kayıt sırasında değil: EF araçları (migrations add) Program.cs'i
                // çalıştırır ama bus'ı başlatmaz, RabbitMQ ayarı olmadan da çalışabilmeli.
                var (rabbitUri, username, password) = ReadConnection(builder.Configuration);
                cfg.Host(rabbitUri.Host, (ushort)rabbitUri.Port, "/", host =>
                {
                    host.Username(username);
                    host.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

    private static (Uri Uri, string Username, string Password) ReadConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} (ör. amqp://kullanici:sifre@localhost:5673) user-secrets'ta tanımlı olmalı; scripts/dev-infra/set-dev-secrets.ps1 yazar.");

        var parts = uri.UserInfo.Split(':', 2);
        if (parts.Length != 2 || parts[0].Length == 0)
            throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} kullanıcı adı ve şifre içermeli (amqp://kullanici:sifre@host:port).");

        // Şifredeki özel karakterler (@, :, / vb.) URL'de kodlanmış gelir.
        return (uri, Uri.UnescapeDataString(parts[0]), Uri.UnescapeDataString(parts[1]));
    }
}
