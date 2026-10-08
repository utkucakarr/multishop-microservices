using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Context;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.BuildingBlocks.Hosting;
using MultiShop.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.AddMultiShopServiceDefaults("Comment");
builder.AddMultiShopJwtAuthentication("ResourceComment");
builder.Services.AddMultiShopAuthorization(fullScope: "CommentFullPermission", readScope: "CommentReadPermission");

builder.Services.AddDbContext<CommentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks().AddDbContextCheck<CommentContext>("sqlserver");

builder.Services.AddControllers();
builder.Services.AddMultiShopSwagger("MultiShop Comment API");

var app = builder.Build();

app.UseMultiShopServiceDefaults();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
