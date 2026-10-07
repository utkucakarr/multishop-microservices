using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using MultiShop.Basket.HealthChecks;
using MultiShop.Basket.Services;
using MultiShop.Basket.Settings;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Basket");
builder.AddMultiShopJwtAuthentication("ResourceBasket");

// Sepet kullanıcıya özel olduğu için tüm uçlar giriş yapmış kullanıcı ister.
var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<RedisService>(sp =>
{
    var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
    var redis = new RedisService(redisSettings.Host, redisSettings.Port, redisSettings.Password);
    redis.Connect();
    return redis;
});

builder.Services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", timeout: TimeSpan.FromSeconds(5));

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
});
builder.Services.AddMultiShopSwagger("MultiShop Basket API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
