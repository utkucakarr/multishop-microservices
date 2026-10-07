using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MultiShop.BuildingBlocks.Swagger;

public static class SwaggerExtensions
{
    /// <summary>Swagger UI'a "Authorize" butonu ekler; girilen token tüm isteklere Bearer olarak eklenir.</summary>
    public static IServiceCollection AddMultiShopSwagger(this IServiceCollection services, string title)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "IdentityServer'dan alınan access token (başına \"Bearer\" yazmadan).",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerScheme] = [] });
        });

        return services;
    }
}
