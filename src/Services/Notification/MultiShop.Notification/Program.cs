using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;
using MultiShop.Notification.Channels.Email;
using MultiShop.Notification.Channels.Sms;
using MultiShop.Notification.Hubs;
using MultiShop.Notification.Notifications;

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

builder.Services.AddSignalR();
builder.Services.AddSingleton<IAdminNotifier, SignalRAdminNotifier>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
// Gerçek SMS sağlayıcısı KT-13'te; şimdilik mesaj yalnızca loglanır.
builder.Services.AddSingleton<ISmsSender, LoggingSmsSender>();

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Notification API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.Path);

app.Run();
