using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultiShop.Discount.Context;
using MultiShop.Discount.Services;

var builder = WebApplication.CreateBuilder(args);

// Re-added after environment variables so this project's user-secrets win over
// any machine/user-level env vars left by other local projects (e.g. a stray
// ConnectionStrings__DefaultConnection).
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true);
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.Authority = builder.Configuration["IdentityServerUrl"];
    opt.Audience = "ResourceDiscount";
    opt.RequireHttpsMetadata = false;
});

// Add services to the container.
builder.Services.AddTransient<DapperContext>();
builder.Services.AddTransient<IDiscountService, DiscountService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
