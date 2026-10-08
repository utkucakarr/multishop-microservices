using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;
using MultiShop.Catalog.HealthChecks;
using MultiShop.Catalog.Repositories.Concrete;
using MultiShop.Catalog.Security;
using MultiShop.Catalog.Repositories.Interfaces;
using MultiShop.Catalog.Services.AboutServices;
using MultiShop.Catalog.Services.BrandServices;
using MultiShop.Catalog.Services.CategoryServices;
using MultiShop.Catalog.Services.ContactServices;
using MultiShop.Catalog.Services.FeatureServices;
using MultiShop.Catalog.Services.FeatureSliderServices;
using MultiShop.Catalog.Services.OfferDiscountServices;
using MultiShop.Catalog.Services.ProductDetailServices;
using MultiShop.Catalog.Services.ProductImageServices;
using MultiShop.Catalog.Services.ProductServices;
using MultiShop.Catalog.Services.SpecialOfferServices;
using MultiShop.Catalog.Services.StatisticServices;
using MultiShop.Catalog.Settings;
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
builder.Services.AddMultiShopAuthorization(fullScope: "CatalogFullPermission", readScope: "CatalogReadPermission")
    .AddPolicy(CatalogPolicies.ContactWrite, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => AuthorizationExtensions.HasAnyScope(context.User, "ContactWritePermission", "CatalogFullPermission")));

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
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();

//Otomapper i�in konfig�rasyon i�lemi
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DataBaseSettings"));
builder.Services.AddScoped<IDatabaseSettings>(sp =>
{
    return sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
});

// Mongo erişilemezken sunucu seçimi 30 sn bekler; health check daha erken yanıt versin.
builder.Services.AddHealthChecks().AddCheck<MongoHealthCheck>("mongodb", timeout: TimeSpan.FromSeconds(5));

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Catalog API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
