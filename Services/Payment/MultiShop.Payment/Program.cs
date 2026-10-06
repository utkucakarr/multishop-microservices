using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Context;
using MultiShop.Payment.Repositories;
using MultiShop.Payment.Services.PaymentServices;
using MultiShop.Payment.Settings;

var builder = WebApplication.CreateBuilder(args);

// Re-added after environment variables so this project's user-secrets win over
// any machine/user-level env vars left by other local projects (e.g. a stray
// ConnectionStrings__DefaultConnection).
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true);
}

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.Configure<GarantiPosSettings>(builder.Configuration.GetSection("GarantiPosSettings"));

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();
