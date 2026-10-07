using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultiShop.BuildingBlocks.HealthChecks;
using MultiShop.BuildingBlocks.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMultiShopSerilog("Gateway");

builder.Services.AddAuthentication().AddJwtBearer("OcelotAuthenticationSheme", opt =>
{
    opt.Authority = builder.Configuration["IdentityServerUrl"];
    opt.Audience = "ResourceOcelot";
    opt.RequireHttpsMetadata = false;
});

// ocelot.json proje klasöründen (content root) okunur; çalışma dizinine bağlı değildir.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Ocelot tüm istekleri yakaladığı için /health ondan önce eklenmeli.
app.UseMultiShopHealthChecks();
app.UseSerilogRequestLogging();

await app.UseOcelot();

app.Run();
