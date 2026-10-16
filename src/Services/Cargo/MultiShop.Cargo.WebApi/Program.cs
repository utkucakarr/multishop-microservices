using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Cargo.BusinessLayer.Services;
using MultiShop.Cargo.DataAccessLayer.Concrete;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.Cargo.WebApi.Consumers;
using MultiShop.Contracts.Cargo;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Cargo");
builder.AddMultiShopJwtAuthentication("ResourceCargo");
builder.Services.AddMultiShopAuthorization(fullScope: "CargoFullPermission");

builder.Services.AddDbContext<CargoContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// İş kuralları BusinessLayer'da; servisler CargoContext'i doğrudan kullanır (generic Manager/Dal katmanları kaldırıldı, CAR-07).
builder.Services.AddScoped<CargoCompanyService>();
builder.Services.AddScoped<ShipmentService>();

builder.Services.AddHealthChecks().AddDbContextCheck<CargoContext>("sqlserver");

// Sipariş saga'sıyla mesajlaşma (KT-13): CreateShipment komutu gönderiyi açar; kargoya verme ve teslim event'leri
// gönderi durumuyla aynı transaction'da outbox'a yazılır. Inbox: aynı komut ikinci kez işlenmez.
builder.AddMultiShopMessaging("Cargo", x =>
{
    x.AddEntityFrameworkOutbox<CargoContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.UseBusOutbox();
    });
    x.AddConfigureEndpointsCallback((context, _, endpoint) => endpoint.UseEntityFrameworkOutbox<CargoContext>(context));

    // Kuyruk adı Contracts'taki sabitle: saga komutu Send ile doğrudan bu kuyruğa gönderir.
    x.AddConsumer<CreateShipmentConsumer>().Endpoint(e => e.Name = CargoQueues.CreateShipment);
});

// Enum'lar JSON'da metin ("Shipped"): yanıtlar okunabilir, WebUI sayılara bağımlı olmaz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMultiShopSwagger("MultiShop Cargo API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
