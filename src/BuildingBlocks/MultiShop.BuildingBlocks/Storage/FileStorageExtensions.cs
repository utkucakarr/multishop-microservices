using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MultiShop.BuildingBlocks.Storage;

public static class FileStorageExtensions
{
    /// <summary>Local depolamada dosyaların sunulduğu yol; gateway'deki <c>/uploads</c> rotası buraya gelir.</summary>
    public const string LocalRequestPath = "/uploads";

    /// <summary>
    /// <see cref="IFileStorage"/>'ı <c>FileStorage</c> ayarına göre kaydeder (varsayılan: Local).
    /// Eksik ayar uygulama açılırken hata verir, ilk yüklemede değil.
    /// </summary>
    public static IServiceCollection AddMultiShopFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(FileStorageOptions.SectionName);

        services.AddOptions<FileStorageOptions>()
            .Bind(section)
            .Validate(o => o.Provider != FileStorageProvider.Local || Uri.IsWellFormedUriString(o.PublicBaseUrl, UriKind.Absolute),
                "FileStorage:PublicBaseUrl (ör. http://localhost:5020/uploads) Local depolama için zorunlu.")
            .Validate(o => o.Provider != FileStorageProvider.Google || !string.IsNullOrWhiteSpace(o.Google.BucketName),
                "FileStorage:Google:BucketName Google depolama için zorunlu.")
            .ValidateOnStart();

        var provider = section.GetValue(nameof(FileStorageOptions.Provider), FileStorageProvider.Local);
        if (provider == FileStorageProvider.Google)
            services.AddSingleton<IFileStorage, GoogleCloudFileStorage>();
        else
            services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }

    /// <summary>
    /// Local depolamada kaydedilen dosyaları <c>/uploads</c> altında sunar (Google'da bir şey yapmaz).
    /// Kimlik doğrulamadan önce çağrılmalı: tarayıcı <c>&lt;img&gt;</c> isteğine token ekleyemez.
    /// </summary>
    public static WebApplication UseMultiShopFileStorage(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;
        if (options.Provider != FileStorageProvider.Local)
            return app;

        var rootPath = LocalFileStorage.GetRootPath(options, app.Environment);
        Directory.CreateDirectory(rootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPath),
            RequestPath = LocalRequestPath,
            // Kullanıcının yüklediği dosya, tarayıcı tarafından içeriği tahmin edilip başka bir türde (ör. HTML) çalıştırılmasın.
            OnPrepareResponse = context => context.Context.Response.Headers.XContentTypeOptions = "nosniff",
        });

        return app;
    }
}
