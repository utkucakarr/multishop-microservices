namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>
/// İstek doğrulamadan geçmedi; alan bazlı hatalarla 400 Bad Request (<c>ValidationProblemDetails</c>) olarak döner.
/// <c>detail</c> tüm mesajları birleştirir: hatayı tek metin olarak gösteren istemciler (WebUI) de okuyabilsin.
/// </summary>
public class ValidationFailedException : BadRequestException
{
    public ValidationFailedException(IReadOnlyDictionary<string, string[]> errors)
        : base(string.Join(" ", errors.SelectMany(x => x.Value)))
    {
        Errors = errors;
    }

    /// <summary>Alan adı → o alanın hata mesajları.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
