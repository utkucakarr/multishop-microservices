using MassTransit;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Application.Sagas.Activities;
using MultiShop.Order.Application.Services;
using MultiShop.Order.Persistence.Context;
using MultiShop.Order.Persistence.Repositories;
using MultiShop.Order.WebApi.BackgroundServices;
using MultiShop.Order.WebApi.Security;
using MultiShop.Order.WebApi.Validation;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Order");
builder.AddMultiShopJwtAuthentication("ResourceOrder");
builder.Services.AddMultiShopAuthorization(fullScope: "OrderFullPermission");

builder.Services.AddDbContext<OrderContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IOrderingRepository), typeof(OrderingRepository));
// Tüm sorgu ve komutlar MediatR üzerinden (ORD-07); handler'lar ve doğrulayıcılar Application'dan otomatik kaydedilir.
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<OrderAccessGuard>();

builder.Services.AddHealthChecks().AddDbContextCheck<OrderContext>("sqlserver");

// Sipariş saga'sı (KT-12). Saga kaydı, sipariş ve outbox aynı OrderContext'te: bir mesajın tüm etkisi tek transaction'da
// yazılır, saga'nın gönderdiği komut/event'ler commit'ten sonra RabbitMQ'ya gider. Inbox: aynı mesaj ikinci kez işlenmez.
builder.Services.AddScoped<CreateOrderingActivity>();
builder.Services.AddScoped<SyncOrderingStatusActivity>();
builder.Services.AddScoped<StartShipmentActivity>();

builder.AddMultiShopMessaging("Order", x =>
{
    x.AddEntityFrameworkOutbox<OrderContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.UseBusOutbox();
    });
    x.AddConfigureEndpointsCallback((context, _, endpoint) => endpoint.UseEntityFrameworkOutbox<OrderContext>(context));

    // İyimser eşzamanlılık (RowVersion): aynı siparişin iki mesajı çakışırsa biri retry ile yeniden işlenir.
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(repository =>
        {
            repository.ConcurrencyMode = ConcurrencyMode.Optimistic;
            repository.ExistingDbContext<OrderContext>();
            repository.UseSqlServer();
        });
});

// Ödenmeyen siparişler süre dolunca iptal edilir, ayrılan stok bırakılır (OrderSettings:PaymentTimeoutMinutes).
builder.Services.Configure<OrderSettings>(builder.Configuration.GetSection(OrderSettings.SectionName));
builder.Services.AddHostedService<OrderPaymentTimeoutService>();

// Enum'lar JSON'da metin ("AwaitingPayment"): yanıtlar okunabilir, WebUI sayılara bağımlı olmaz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMultiShopSwagger("MultiShop Order API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
