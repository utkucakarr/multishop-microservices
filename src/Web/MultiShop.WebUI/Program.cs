using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using MultiShop.BuildingBlocks.HealthChecks;
using MultiShop.BuildingBlocks.Logging;
using MultiShop.WebUI.Handlers;
using MultiShop.WebUI.Middlewares;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CargoServices.CargoCompanyServices;
using MultiShop.WebUI.Services.CargoServices.ShipmentServices;
using MultiShop.WebUI.Services.CatalogServices.AboutServices;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;
using MultiShop.WebUI.Services.CatalogServices.FeatureSliderServices;
using MultiShop.WebUI.Services.CatalogServices.FeautureServices;
using MultiShop.WebUI.Services.CatalogServices.OfferDiscountServices;
using MultiShop.WebUI.Services.CatalogServices.ProductDetailServices;
using MultiShop.WebUI.Services.CatalogServices.ProductImageServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.CatalogServices.SpecialOfferServices;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Concrete;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;
using MultiShop.WebUI.Services.NotificationServices;
using MultiShop.WebUI.Services.OrderServices.OrderAddressServices;
using MultiShop.WebUI.Services.OrderServices.OrderOrderingServices;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.StatisticServices;
using MultiShop.WebUI.Services.UserIdentityServices;
using MultiShop.WebUI.Settings;
using MultiShop.WebUI.Services.PaymentServices;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseMultiShopSerilog("WebUI");
builder.Services.AddHealthChecks();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
{
    opt.LoginPath = "/Login/Index/";
    // Giriş yapmış ama yetkisi olmayan kullanıcı (ör. müşteri /Admin/...) bu sayfaya yönlenir.
    opt.AccessDeniedPath = "/Login/AccessDenied/";
    opt.ExpireTimeSpan = TimeSpan.FromDays(5);
    opt.Cookie.Name = "MultiShopCookie";
    opt.SlidingExpiration = true;
});

builder.Services.AddAccessTokenManagement();

builder.Services.AddHttpContextAccessor();

// GET/HEAD/OPTIONS dışındaki her istek antiforgery token'ı taşımalı (form tag helper'ı token'ı kendisi ekler).
builder.Services.AddControllersWithViews(opt => opt.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddRazorRuntimeCompilation()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Giriş yapan kullanıcının cookie claim'leri (ad, e-posta, rol); Identity'ye istek atmaz.
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddHttpClient<IIdentityService, IdentityService>();

builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
builder.Services.Configure<ServiceApiSettings>(builder.Configuration.GetSection("ServiceApiSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddScoped<IUserAccessTokenService, UserAccessTokenService>();
// Kullanıcıya özel servisler (sepet, sipariş, admin istatistikleri...): giriş yapmış kullanıcının token'ı.
builder.Services.AddTransient<ResourceOwnerPasswordTokenHandler>();
// Herkese açık sayfalarda da kullanılan servisler (Catalog, Comment): kullanıcı varsa onun, yoksa ziyaretçi token'ı.
builder.Services.AddTransient<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IClientCredentialTokenService, ClientCredentialTokenService>();

var values = builder.Configuration.GetSection("ServiceApiSettings").Get<ServiceApiSettings>();

builder.Services.AddHttpClient<IUserService, UserService>(opt =>
{
    opt.BaseAddress = new Uri(values.IdentityServerUrl);
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IUserIdentityService, UserIdentityService>(opt =>
{
    opt.BaseAddress = new Uri(values.IdentityServerUrl);
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

// Üye kaydı: IdentityServer'ın herkese açık ucu, token gerekmez.
builder.Services.AddHttpClient<IRegisterService, RegisterService>(opt =>
{
    opt.BaseAddress = new Uri(values.IdentityServerUrl);
});

// Admin istatistikleri: dashboard tek istekte (Ocelot aggregate /services/admin/dashboard) + üst menü için tekil uçlar.
builder.Services.AddHttpClient<IDashboardService, DashboardService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/services/");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IBasketService, BasketService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Basket.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IOrderOrderingService, OrderOrderingService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Order.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<ICargoCompanyService, CargoCompanyService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Cargo.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IShipmentService, ShipmentService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Cargo.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IOrderAddressService, OrderAddressService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Order.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IOrderDetailService, OrderDetailService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Order.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<IPaymentService, PaymentService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Payment.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddHttpClient<ICategoryService, CategoryService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IProductService, ProductService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<ISpecialOfferService, SpecialOfferService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IFeatureSliderService, FeatureSliderService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IFeatureService, FeatureService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IOfferDiscountService, OfferDiscountService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IBrandService, BrandService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IAboutService, AboutService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IProductImageService, ProductImageService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<IProductDetailService, ProductDetailService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Catalog.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

builder.Services.AddHttpClient<ICommentService, CommentService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Comment.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

// İletişim formu ziyaretçi token'ıyla gönderilir; admin ekranları admin'in kendi token'ını kullanır.
builder.Services.AddHttpClient<IContactMessageService, ContactMessageService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Message.Path}");
}).AddHttpMessageHandler<UserOrVisitorTokenHandler>();

// Admin bildirim kutusu ve ayarları (Notification servisi): admin'in kendi token'ı.
builder.Services.AddHttpClient<INotificationService, NotificationService>(opt =>
{
    opt.BaseAddress = new Uri($"{values.OcelotUrl}/{values.Notification.Path}");
}).AddHttpMessageHandler<ResourceOwnerPasswordTokenHandler>();

builder.Services.AddLocalization(opt =>
{
    opt.ResourcesPath = "Resources"; 
});

var app = builder.Build();

// Development'ta beklenmeyen kod hataları geliştirici sayfasında görünür; servis hataları her ortamda ServiceExceptionMiddleware'de.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMultiShopHealthChecks();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMultiShopRequestLogging();

// Gövdesiz hata yanıtları ErrorController.Status'a gider; yalnızca 404 ve 400 için sayfa gösterilir.
app.UseStatusCodePagesWithReExecute("/Error/{0}");
// UseRouting'den önce: hata sayfasını isteği yeniden yönlendirerek üretir.
app.UseMiddleware<ServiceExceptionMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[] { "en", "fr", "tr" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[2]).AddSupportedCultures
    (supportedCultures).AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

app.Run();