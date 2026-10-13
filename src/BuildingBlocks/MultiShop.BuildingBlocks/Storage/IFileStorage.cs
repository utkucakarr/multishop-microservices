namespace MultiShop.BuildingBlocks.Storage;

/// <summary>
/// Dosyaların (ör. ürün görselleri) saklandığı yer. Yerel disk ya da Google Cloud Storage olabilir;
/// hangisinin kullanılacağı <c>FileStorage:Provider</c> ayarından seçilir.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Dosyayı <paramref name="folder"/> altına rastgele bir adla kaydeder ve herkese açık URL'sini döner.
    /// Veritabanına bu URL yazılır.
    /// </summary>
    /// <param name="folder">Kodda sabit bir klasör adı, ör. <c>products</c>. Kullanıcıdan alınmamalı.</param>
    /// <param name="extension">Noktayla birlikte uzantı, ör. <c>.jpg</c>.</param>
    Task<string> SaveAsync(Stream content, string folder, string extension, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL bu depolamaya aitse dosyayı siler. Elle girilmiş dış URL'lerde (ör. eski veriler) hiçbir şey yapmaz;
    /// dosya zaten yoksa hata vermez.
    /// </summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}
