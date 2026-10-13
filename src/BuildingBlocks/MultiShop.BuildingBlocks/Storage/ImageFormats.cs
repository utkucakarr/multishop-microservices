namespace MultiShop.BuildingBlocks.Storage;

public sealed record ImageFormat(string Extension, string ContentType);

/// <summary>
/// Yüklenen dosyanın gerçekten görsel olup olmadığını dosya adına ya da tarayıcının gönderdiği
/// Content-Type'a değil, dosyanın ilk byte'larına bakarak anlar.
/// </summary>
public static class ImageFormats
{
    public static readonly ImageFormat Jpeg = new(".jpg", "image/jpeg");
    public static readonly ImageFormat Png = new(".png", "image/png");
    public static readonly ImageFormat Webp = new(".webp", "image/webp");

    /// <summary>JPEG, PNG ya da WebP ise biçimini, değilse <c>null</c> döner. Akışın konumu başa alınır.</summary>
    public static async Task<ImageFormat?> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[12];
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        stream.Position = 0;

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return Jpeg;
        if (read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return Png;
        if (read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return Webp;

        return null;
    }
}
