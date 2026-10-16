using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Context;
using MultiShop.Comment.Services;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Comment");
builder.AddMultiShopJwtAuthentication("ResourceComment");
builder.Services.AddMultiShopAuthorization(fullScope: "CommentFullPermission", readScope: "CommentReadPermission");

builder.Services.AddDbContext<CommentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CommentService>();

builder.Services.AddHealthChecks().AddDbContextCheck<CommentContext>("sqlserver");

// Transactional outbox: CommentCreated yorumla aynı transaction'da OutboxMessage tablosuna yazılır,
// arka plandaki gönderici RabbitMQ'ya iletir. RabbitMQ kapalıysa event kaybolmaz, açılınca gider.
builder.AddMultiShopMessaging("Comment", x =>
    x.AddEntityFrameworkOutbox<CommentContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.UseBusOutbox();
    }));

// Enum'lar JSON'da metin ("Pending"): WebUI sayılara bağımlı olmaz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMultiShopSwagger("MultiShop Comment API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
