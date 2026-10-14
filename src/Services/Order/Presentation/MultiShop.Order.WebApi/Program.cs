using MassTransit;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MultiShop.Order.Application.Features.CQRS.Handlers.AddressHandlers;
using MultiShop.Order.Application.Features.CQRS.Handlers.OrderDetailHandlers;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Application.Sagas;
using MultiShop.Order.Application.Sagas.Activities;
using MultiShop.Order.Application.Services;
using MultiShop.Order.Persistence.Context;
using MultiShop.Order.Persistence.Repositories;
using MultiShop.Order.WebApi.BackgroundServices;
using MultiShop.Order.WebApi.Security;
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
builder.Services.AddScoped(typeof(IOrderDetailRepository), typeof(OrderDetailRepository));
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddScoped<OrderAccessGuard>();

#region
builder.Services.AddScoped<GetAddressQueryHandler>();
builder.Services.AddScoped<GetAddressByIdQueryHandler>();
builder.Services.AddScoped<CreateAddressCommandHandler>();
builder.Services.AddScoped<UpdateAddressCommandHandler>();
builder.Services.AddScoped<RemoveAddressCommandHandler>();

builder.Services.AddScoped<GetOrderDetailQueryHandler>();
builder.Services.AddScoped<GetOrderDetailByIdQueryHandler>();
builder.Services.AddScoped<UpdateOrderDetailQueryHandler>();
builder.Services.AddScoped<GetOrderDetailByOrderingIdQueryHandler>();
builder.Services.AddScoped<RemoveOrderDetailQueryHandler>();
#endregion

builder.Services.AddHealthChecks().AddDbContextCheck<OrderContext>("sqlserver");

// Sipariş saga'sı (KT-12). Saga kaydı, sipariş ve outbox aynı OrderContext'te: bir mesajın tüm etkisi tek transaction'da
// yazılır, saga'nın gönderdiği komut/event'ler commit'ten sonra RabbitMQ'ya gider. Inbox: aynı mesaj ikinci kez işlenmez.
builder.Services.AddScoped<CreateOrderingActivity>();
builder.Services.AddScoped<SyncOrderingStatusActivity>();

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
