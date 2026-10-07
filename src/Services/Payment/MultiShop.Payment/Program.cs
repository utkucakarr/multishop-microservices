using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Context;
using MultiShop.Payment.Repositories;
using MultiShop.Payment.Services.PaymentServices;
using MultiShop.Payment.Settings;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Payment");

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.Configure<GarantiPosSettings>(builder.Configuration.GetSection("GarantiPosSettings"));

builder.Services.AddHealthChecks().AddDbContextCheck<PaymentContext>("sqlserver");

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Payment API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthorization();

app.MapControllers();

app.Run();
