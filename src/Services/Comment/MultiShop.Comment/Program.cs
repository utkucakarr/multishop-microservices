using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Context;

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
    opt.Audience = "ResourceComment";
    opt.RequireHttpsMetadata = false;
});

// Add services to the container.

builder.Services.AddDbContext<CommentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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