using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiShop.IdentityServer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiShop.IdentityServer
{
    /// <summary>
    /// Her açılışta çalışır ve tekrar çalıştırılması güvenlidir:
    /// 1. Admin ve Customer rollerini oluşturur.
    /// 2. <c>AdminUser:Email</c> adresindeki kullanıcıya Admin rolü verir; kullanıcı yoksa
    ///    <c>AdminUser:UserName</c> ve <c>AdminUser:Password</c> ile oluşturur.
    /// 3. Hiç rolü olmayan kullanıcılara Customer rolü verir.
    /// </summary>
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IdentitySeeder));

            try
            {
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = provider.GetRequiredService<IConfiguration>();

                foreach (var role in Roles.All)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)), $"'{role}' rolü oluşturulamadı");
                        logger.LogInformation("Rol oluşturuldu: {Role}", role);
                    }
                }

                await EnsureAdminAsync(userManager, configuration, logger);

                var assigned = 0;
                foreach (var user in await userManager.Users.ToListAsync())
                {
                    if ((await userManager.GetRolesAsync(user)).Count == 0)
                    {
                        EnsureSucceeded(await userManager.AddToRoleAsync(user, Roles.Customer), $"{user.UserName} kullanıcısına Customer rolü verilemedi");
                        assigned++;
                    }
                }

                if (assigned > 0)
                    logger.LogInformation("{Count} kullanıcıya Customer rolü verildi", assigned);
            }
            catch (Exception ex)
            {
                // Veritabanına ulaşılamasa da IdentityServer açılsın; durum /health'te görünür.
                logger.LogError(ex, "Rol ve admin tohumlama başarısız");
            }
        }

        private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
        {
            var email = configuration["AdminUser:Email"];
            if (string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("'AdminUser:Email' ayarı yok; hiçbir kullanıcı Admin yapılmadı");
                return;
            }

            var admin = await userManager.FindByEmailAsync(email);
            if (admin is null)
            {
                var password = configuration["AdminUser:Password"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("{Email} adresli kullanıcı yok ve 'AdminUser:Password' verilmemiş; admin oluşturulmadı", email);
                    return;
                }

                admin = new ApplicationUser
                {
                    UserName = configuration["AdminUser:UserName"] ?? email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = "Admin",
                    Surname = "MultiShop"
                };
                EnsureSucceeded(await userManager.CreateAsync(admin, password), "Admin kullanıcısı oluşturulamadı");
                logger.LogInformation("Admin kullanıcısı oluşturuldu: {UserName}", admin.UserName);
            }

            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            {
                EnsureSucceeded(await userManager.AddToRoleAsync(admin, Roles.Admin), "Admin rolü verilemedi");
                logger.LogInformation("{UserName} kullanıcısına Admin rolü verildi", admin.UserName);
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string message)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException($"{message}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
