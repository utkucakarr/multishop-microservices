using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MultiShop.Order.Application.Services
{
    public static class ServiceRegistration
    {
        public static void AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly));

            // Komut doğrulayıcıları; MediatR pipeline'ında çalıştırılır (WebApi: ValidationBehavior), hata mesajları Türkçe.
            services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly, includeInternalTypes: false);
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr");
        }
    }
}
