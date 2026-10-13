using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.Message.DAL.Context;
using MultiShop.Message.Security;
using MultiShop.Message.Services;
using System.Reflection;
// MassTransit'te de MessageContext adında bir tür var; buradaki bizim DbContext'imiz.
using MessageContext = MultiShop.Message.DAL.Context.MessageContext;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Message");
builder.AddMultiShopJwtAuthentication("ResourceMessage");
builder.Services.AddMultiShopAuthorization(fullScope: "MessageFullPermission")
    .AddPolicy(MessagePolicies.ContactWrite, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => AuthorizationExtensions.HasAnyScope(context.User, "ContactWritePermission", "MessageFullPermission")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MessageContext>(opt =>
{
    opt.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddHealthChecks().AddDbContextCheck<MessageContext>("postgresql");

// Transactional outbox: ContactMessageReceived mesajla aynı transaction'da OutboxMessage tablosuna yazılır,
// arka plandaki gönderici RabbitMQ'ya iletir. RabbitMQ kapalıysa event kaybolmaz, açılınca gider.
builder.AddMultiShopMessaging("Message", x =>
    x.AddEntityFrameworkOutbox<MessageContext>(outbox =>
    {
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    }));

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Message API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
