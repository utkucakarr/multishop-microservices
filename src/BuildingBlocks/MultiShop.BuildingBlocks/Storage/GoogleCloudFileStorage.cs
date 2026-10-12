using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace MultiShop.BuildingBlocks.Storage;

/// <summary>
/// Dosyaları Google Cloud Storage bucket'ına yazar. Görsellerin tarayıcıda açılabilmesi için bucket'ın
/// herkese açık okunur olması gerekir (allUsers → Storage Object Viewer).
/// </summary>
public class GoogleCloudFileStorage : FileStorageBase
{
    private readonly StorageClient _client;
    private readonly string _bucketName;

    public GoogleCloudFileStorage(IOptions<FileStorageOptions> options)
        : base(GetPublicBaseUrl(options.Value))
    {
        var google = options.Value.Google;
        _bucketName = google.BucketName!;

        // StorageClient thread-safe'tir; her istekte yeniden oluşturmak yerine tek örnek kullanılır.
        var credential = string.IsNullOrWhiteSpace(google.CredentialFile)
            ? GoogleCredential.GetApplicationDefault()
            : GoogleCredential.FromFile(google.CredentialFile);
        _client = StorageClient.Create(credential);
    }

    private static string GetPublicBaseUrl(FileStorageOptions options)
        => string.IsNullOrWhiteSpace(options.PublicBaseUrl)
            ? $"https://storage.googleapis.com/{options.Google.BucketName}"
            : options.PublicBaseUrl;

    protected override Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
        => _client.UploadObjectAsync(_bucketName, key, contentType, content, cancellationToken: cancellationToken);

    protected override async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteObjectAsync(_bucketName, key, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            // Dosya zaten yok.
        }
    }
}
