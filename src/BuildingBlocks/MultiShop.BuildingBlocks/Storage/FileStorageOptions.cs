namespace MultiShop.BuildingBlocks.Storage;

public enum FileStorageProvider
{
    Local,
    Google,
}

/// <summary><c>FileStorage</c> ayar bölümü.</summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public FileStorageProvider Provider { get; set; } = FileStorageProvider.Local;

    /// <summary>
    /// Kaydedilen dosyaların dışarıdan erişildiği adres; URL'ler bunun üzerine kurulur.
    /// Local'de zorunlu (ör. gateway üzerinden <c>http://localhost:5020/uploads</c>);
    /// Google'da boş bırakılırsa <c>https://storage.googleapis.com/{bucket}</c> kullanılır.
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>Local: dosyaların yazıldığı klasör, servisin içerik köküne göre.</summary>
    public string LocalRootPath { get; set; } = "uploads";

    public GoogleCloudStorageOptions Google { get; set; } = new();
}

public class GoogleCloudStorageOptions
{
    public string? BucketName { get; set; }

    /// <summary>
    /// Service account JSON dosyasının yolu (user-secrets'ta tutulur). Boşsa ortamın varsayılan
    /// kimliği kullanılır (Application Default Credentials, ör. Cloud Run'da servis hesabı).
    /// </summary>
    public string? CredentialFile { get; set; }
}
