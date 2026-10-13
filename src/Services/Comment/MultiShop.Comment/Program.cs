using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Context;
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

builder.Services.AddHealthChecks().AddDbContextCheck<CommentContext>("sqlserver");

// Transactional outbox: CommentCreated yorumla aynı transaction'da OutboxMessage tablosuna yazılır,
// arka plandaki gönderici RabbitMQ'ya iletir. RabbitMQ kapalıysa event kaybolmaz, açılınca gider.
builder.AddMultiShopMessaging("Comment", x =>
    x.AddEntityFrameworkOutbox<CommentContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.UseBusOutbox();
    }));

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Comment API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
