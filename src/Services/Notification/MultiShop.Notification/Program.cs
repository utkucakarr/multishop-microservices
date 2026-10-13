using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;
using MultiShop.Notification.Channels;
using MultiShop.Notification.Channels.Email;
using MultiShop.Notification.Channels.InApp;
using MultiShop.Notification.Channels.Sms;
using MultiShop.Notification.Data;
using MultiShop.Notification.Dispatching;
using MultiShop.Notification.Hubs;
using MultiShop.Notification.Services;
using MultiShop.Notification.Templates;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Notification");
builder.AddMultiShopJwtAuthentication("ResourceNotification");
builder.Services.AddMultiShopAuthorization(fullScope: "NotificationFullPermission");

// Tarayıcı WebSocket bağlantısına Authorization başlığı ekleyemez; SignalR istemcisi token'ı
// ?access_token= ile gönderir. Yalnızca hub yolunda query string'den okunur (başka uçlarda URL'de token kabul edilmez).
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(NotificationHub.Path))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    });

// Kalıcı bildirimler, gönderim outbox'ı, kurallar ve alıcılar (PostgreSQL, MultiShopNotificationDb).
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHealthChecks().AddDbContextCheck<NotificationDbContext>("postgresql");

builder.Services.AddSignalR();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
// Gerçek SMS sağlayıcısı KT-13'te; şimdilik mesaj yalnızca loglanır (gönderim durumu "Simulated").
builder.Services.AddSingleton<ISmsSender, LoggingSmsSender>();

// Şablonlar (tür başına metinler).
builder.Services.AddSingleton<INotificationTemplate, TestTemplate>();
builder.Services.AddSingleton<INotificationTemplate, ContactMessageReceivedTemplate>();
builder.Services.AddSingleton<INotificationTemplate, CommentCreatedTemplate>();
builder.Services.AddSingleton<NotificationTemplateRegistry>();

// Strategy: kanallar. Yeni kanal = yeni INotificationChannel + bir satır kayıt.
builder.Services.AddSingleton<INotificationChannel, InAppChannel>();
builder.Services.AddSingleton<INotificationChannel, EmailChannel>();
builder.Services.AddSingleton<INotificationChannel, SmsChannel>();
builder.Services.AddSingleton<NotificationChannelRegistry>();

// Dispatcher + gönderim outbox'ının işlemcisi.
builder.Services.AddSingleton<DeliverySignal>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddHostedService<DeliveryProcessor>();

// Admin paneli REST API'si (bildirim kutusu, ayarlar).
builder.Services.AddScoped<INotificationInboxService, NotificationInboxService>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();

// Enum'lar JSON'da metin ("Email", "Failed"): API yanıtları okunabilir, WebUI sayılara bağımlı olmaz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMultiShopSwagger("MultiShop Notification API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.Path);

app.Run();
