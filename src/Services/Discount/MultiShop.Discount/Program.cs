using Microsoft.EntityFrameworkCore;
using MultiShop.Discount.Context;
using MultiShop.Discount.Services;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Discount");
builder.AddMultiShopJwtAuthentication("ResourceDiscount");
builder.Services.AddMultiShopAuthorization(fullScope: "DiscountFullPermission");

// Sorgular Dapper (bağlantı fabrikası), şema ve health check EF Core (DIS-04).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanımlı değil.");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddDbContext<DiscountDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IDiscountService, DiscountService>();

builder.Services.AddHealthChecks().AddDbContextCheck<DiscountDbContext>("sqlserver");

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Discount API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
