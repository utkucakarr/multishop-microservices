using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Context;
using MultiShop.Payment.Consumers;
using MultiShop.Payment.Services.GarantiPosServices;
using MultiShop.Payment.Services.PaymentServices;
using MultiShop.Payment.Settings;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Payment");
// Önceden Payment'ta hiç kimlik doğrulama yoktu (G-09).
builder.AddMultiShopJwtAuthentication("ResourcePayment");
builder.Services.AddMultiShopAuthorization(fullScope: "PaymentFullPermission");

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<GarantiPosSettings>(builder.Configuration.GetSection("GarantiPosSettings"));
// Banka isteği tekrar denenmez (satış idempotent değil); 30 sn içinde cevap gelmezse sonuç belirsiz sayılır.
builder.Services.AddHttpClient<IPosClient, GarantiPosClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddHealthChecks().AddDbContextCheck<PaymentContext>("sqlserver");

// Sipariş saga'sıyla mesajlaşma (KT-12): OrderAwaitingPayment ödeme kaydını açar, OrderCancelled kapatır.
// Outbox: PaymentSucceeded/PaymentFailed ödeme sonucuyla aynı transaction'da yazılır. Inbox: aynı event ikinci kez işlenmez.
builder.AddMultiShopMessaging("Payment", x =>
{
    x.AddEntityFrameworkOutbox<PaymentContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.UseBusOutbox();
    });
    x.AddConfigureEndpointsCallback((context, _, endpoint) => endpoint.UseEntityFrameworkOutbox<PaymentContext>(context));

    x.AddConsumer<OrderAwaitingPaymentConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();
});

// Enum'lar JSON'da metin ("AwaitingPayment"): yanıtlar okunabilir, WebUI sayılara bağımlı olmaz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMultiShopSwagger("MultiShop Payment API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
