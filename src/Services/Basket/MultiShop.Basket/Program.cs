using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using MultiShop.Basket.Clients;
using MultiShop.Basket.Data;
using MultiShop.Basket.HealthChecks;
using MultiShop.Basket.Pricing;
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
builder.Services.AddScoped<BasketRepository>();

// Tek fiyat formülü; KDV oranı ve kargo ücreti ayardan.
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection(PricingOptions.SectionName));
builder.Services.AddSingleton<PricingCalculator>();

// Fiyat Catalog'dan, kupon Discount'tan; ikisine de kullanıcının token'ı iletilir.
var serviceUrls = builder.Configuration.GetSection(ServiceUrls.SectionName).Get<ServiceUrls>() ?? new ServiceUrls();
builder.Services.AddTransient<ForwardAccessTokenHandler>();
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(serviceUrls.Catalog);
    client.Timeout = TimeSpan.FromSeconds(5);
}).AddHttpMessageHandler<ForwardAccessTokenHandler>();
builder.Services.AddHttpClient<DiscountClient>(client =>
{
    client.BaseAddress = new Uri(serviceUrls.Discount);
    client.Timeout = TimeSpan.FromSeconds(5);
}).AddHttpMessageHandler<ForwardAccessTokenHandler>();
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
