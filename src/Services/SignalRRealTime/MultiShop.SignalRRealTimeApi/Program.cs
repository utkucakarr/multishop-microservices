using MultiShop.SignalRRealTimeApi.Hubs;
using MultiShop.SignalRRealTimeApi.Services.SignalRCommentServices;
using MultiShop.SignalRRealTimeApi.Services.SignalRMessageServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyHeader() //Her türlü HTTP baþlýðýný kabul eder.
               .AllowAnyMethod() //Tüm HTTP metodlarýna izin verir (GET, POST, PUT, DELETE, vb.).
               .SetIsOriginAllowed((host) => true) // Herhangi bir kaynaktan gelen isteklere izin verir.
               .AllowCredentials(); //Kimlik bilgilerini (çerezler, oturum bilgilerinin) içeren isteklere izin verir.
    });
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISignalRMessageService, SignalRMessageService>();
builder.Services.AddScoped<ISignalRCommentServices, SignalRCommentServices>();

builder.Services.AddSignalR();

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

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<SignalRHub>("/signalrhub");

app.Run();
