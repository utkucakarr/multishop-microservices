using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MultiShop.BuildingBlocks.Storage;

/// <summary>
/// Dosyaları servisin kendi klasörüne (varsayılan <c>uploads/</c>) yazar. Servis bu klasörü
/// <see cref="FileStorageExtensions.UseMultiShopFileStorage"/> ile <c>/uploads</c> altında sunar.
/// </summary>
public class LocalFileStorage : FileStorageBase
{
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options, IHostEnvironment environment)
        : base(options.Value.PublicBaseUrl!)
    {
        _rootPath = GetRootPath(options.Value, environment);
    }

    internal static string GetRootPath(FileStorageOptions options, IHostEnvironment environment)
        => Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.LocalRootPath));

    protected override async Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = GetPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(file, cancellationToken);
    }

    protected override Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        File.Delete(GetPath(key)); // Dosya yoksa hata vermez.
        return Task.CompletedTask;
    }

    private string GetPath(string key) => Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
}
