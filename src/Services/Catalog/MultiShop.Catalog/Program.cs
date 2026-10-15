using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Messaging;
using MultiShop.BuildingBlocks.Storage;
using MultiShop.BuildingBlocks.Swagger;
using MultiShop.Catalog.Consumers;
using MultiShop.Catalog.HealthChecks;
using MultiShop.Catalog.Repositories.Concrete;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Services.AboutServices;
using MultiShop.Catalog.Services.BrandServices;
using MultiShop.Catalog.Services.CategoryServices;
using MultiShop.Catalog.Services.FeatureServices;
using MultiShop.Catalog.Services.FeatureSliderServices;
using MultiShop.Catalog.Services.OfferDiscountServices;
using MultiShop.Catalog.Services.ProductDetailServices;
using MultiShop.Catalog.Services.ProductImageServices;
using MultiShop.Catalog.Services.ProductServices;
using MultiShop.Catalog.Services.SpecialOfferServices;
using MultiShop.Catalog.Services.StatisticServices;
using MultiShop.Catalog.Services.StockServices;
using MultiShop.Catalog.Settings;
using MultiShop.Contracts.Catalog;
using System.Reflection;

// Mongo dokümanlarında entity class'ında karşılığı olmayan eski/fazladan alanlar
// (ör. şema değişiklikleri sonrası kalan eski alan adları) artık hata fırlatmak yerine yok sayılır.
ConventionRegistry.Register(
    "IgnoreExtraElements",
    new ConventionPack { new IgnoreExtraElementsConvention(true) },
    _ => true);

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Catalog");
builder.AddMultiShopJwtAuthentication("ResourceCatalog");
builder.Services.AddMultiShopAuthorization(fullScope: "CatalogFullPermission", readScope: "CatalogReadPermission");

// Entity'lerdeki [BsonRepresentation(BsonType.ObjectId)] alanları, geçersiz bir id ile sorgulanınca
// FormatException fırlatır; bu bir istemci hatasıdır (500 değil 400).
builder.Services.Configure<ExceptionMappingOptions>(opt => opt.Map<FormatException>(StatusCodes.Status400BadRequest));

//Burada ICategoryservice �a��r�ld���nda categoryservice s�n�f�n�n �a��r�lmas�n� sa�l�yoruz
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, MongoCategoryRepository>();
builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductDetailService, ProductDetailService>();
builder.Services.AddScoped<IProductDetailRepository, MongoProductDetailRepository>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IProductImageRepository, MongoProductImageRepository>();
builder.Services.AddScoped<IFeatureSliderService, FeatureSliderService>();
builder.Services.AddScoped<ISpecialOfferService, SpecialOfferService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IOfferDiscountService, OfferDiscountService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IAboutService, AboutService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();

// Ürün görselleri: FileStorage:Provider ayarına göre yerel disk (uploads/) ya da Google Cloud Storage.
builder.Services.AddMultiShopFileStorage(builder.Configuration);

//Otomapper i�in konfig�rasyon i�lemi
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DataBaseSettings"));
builder.Services.AddScoped<IDatabaseSettings>(sp =>
{
    return sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
});

// MongoClient thread-safe'tir ve bağlantı havuzunu kendisi yönetir; uygulama boyunca tek örnek kullanılır
// (MongoDB'nin önerisi). Servisler ve repository'ler hazır IMongoDatabase'i alır.
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(sp.GetRequiredService<IOptions<DatabaseSettings>>().Value.ConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(sp.GetRequiredService<IOptions<DatabaseSettings>>().Value.DatabaseName));

// Sipariş saga'sının stok komutları (KT-12). Mongo outbox/inbox: her consumer bir MongoDB transaction'ında çalışır;
// stok, rezervasyon, inbox kaydı ve yayınlanan event birlikte commit edilir. MongoDB replica set gerektirir (docker-compose).
builder.Services.AddScoped<StockReservationService>();
builder.AddMultiShopMessaging("Catalog", x =>
{
    x.AddMongoDbOutbox(outbox =>
    {
        outbox.ClientFactory(sp => sp.GetRequiredService<IMongoClient>());
        outbox.DatabaseFactory(sp => sp.GetRequiredService<IMongoDatabase>());
    });
    x.AddConfigureEndpointsCallback((context, _, endpoint) => endpoint.UseMongoDbOutbox(context));

    // Kuyruk adları Contracts'taki sabitlerle: saga komutları Send ile doğrudan bu kuyruklara gönderir.
    x.AddConsumer<ReserveStockConsumer>().Endpoint(e => e.Name = CatalogQueues.ReserveStock);
    x.AddConsumer<CommitStockConsumer>().Endpoint(e => e.Name = CatalogQueues.CommitStock);
    x.AddConsumer<ReleaseStockConsumer>().Endpoint(e => e.Name = CatalogQueues.ReleaseStock);
});

// Mongo erişilemezken sunucu seçimi 30 sn bekler; health check daha erken yanıt versin.
builder.Services.AddHealthChecks().AddCheck<MongoHealthCheck>("mongodb", timeout: TimeSpan.FromSeconds(5));

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Catalog API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

// Yerel depolamada görseller /uploads altında token'sız sunulur (tarayıcı <img> isteğine token ekleyemez).
app.UseMultiShopFileStorage();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
