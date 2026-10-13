using System.Text.RegularExpressions;

namespace MultiShop.BuildingBlocks.Storage;

/// <summary>
/// İki depolamanın ortak kısmı: dosya adının üretilmesi ve URL ile dosya anahtarı (<c>products/abc.jpg</c>)
/// arasındaki dönüşüm. Alt sınıflar yalnızca anahtarla yazar ve siler.
/// </summary>
public abstract partial class FileStorageBase : IFileStorage
{
    private readonly string _publicBaseUrl;

    protected FileStorageBase(string publicBaseUrl)
    {
        _publicBaseUrl = publicBaseUrl.TrimEnd('/');
    }

    public async Task<string> SaveAsync(Stream content, string folder, string extension, string contentType, CancellationToken cancellationToken = default)
    {
        if (!FolderRegex().IsMatch(folder))
            throw new ArgumentException($"Geçersiz klasör adı: '{folder}'.", nameof(folder));
        if (!ExtensionRegex().IsMatch(extension))
            throw new ArgumentException($"Geçersiz uzantı: '{extension}'.", nameof(extension));

        // Dosya adı kullanıcının verdiği addan üretilmez: klasör dışına yazdırma (../) ve ad çakışması olmaz.
        var key = $"{folder}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await WriteAsync(key, content, contentType, cancellationToken);
        return $"{_publicBaseUrl}/{key}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var prefix = _publicBaseUrl + "/";
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var key = url[prefix.Length..];
        return KeyRegex().IsMatch(key) ? RemoveAsync(key, cancellationToken) : Task.CompletedTask;
    }

    protected abstract Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);

    protected abstract Task RemoveAsync(string key, CancellationToken cancellationToken);

    [GeneratedRegex("^[a-z0-9-]+(/[a-z0-9-]+)*$")]
    private static partial Regex FolderRegex();

    [GeneratedRegex(@"^\.[a-zA-Z0-9]{1,10}$")]
    private static partial Regex ExtensionRegex();

    // SaveAsync'in ürettiği biçim; başka bir şey (ör. ../) silinmeye çalışılmaz.
    [GeneratedRegex(@"^[a-z0-9-]+(/[a-z0-9-]+)*/[a-f0-9]{32}\.[a-z0-9]{1,10}$")]
    private static partial Regex KeyRegex();
}
