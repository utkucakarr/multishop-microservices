using MultiShop.Discount.Context;
using MultiShop.Discount.Services;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Discount");
builder.AddMultiShopJwtAuthentication("ResourceDiscount");

builder.Services.AddTransient<DapperContext>();
builder.Services.AddTransient<IDiscountService, DiscountService>();

builder.Services.AddHealthChecks().AddDbContextCheck<DapperContext>("sqlserver");

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Discount API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
