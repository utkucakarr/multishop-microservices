using Microsoft.AspNetCore.Http;

namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>
/// Hangi exception'ın hangi HTTP durum koduna çevrileceği. Ortak eşlemeler hazır gelir;
/// bir servis kendine özgü eşleme ekleyebilir:
/// <code>builder.Services.Configure&lt;ExceptionMappingOptions&gt;(o =&gt; o.Map&lt;FormatException&gt;(400));</code>
/// </summary>
public class ExceptionMappingOptions
{
    private readonly List<(Type Type, int StatusCode)> _mappings =
    [
        (typeof(NotFoundException), StatusCodes.Status404NotFound),
        (typeof(DomainException), StatusCodes.Status400BadRequest),
        (typeof(BadRequestException), StatusCodes.Status400BadRequest),
        (typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized),
    ];

    public ExceptionMappingOptions Map<TException>(int statusCode) where TException : Exception
    {
        // Servisin eklediği eşleme ortak eşlemelerden önce denenir.
        _mappings.Insert(0, (typeof(TException), statusCode));
        return this;
    }

    /// <summary>Eşleşme yoksa 500 döner. Alt sınıflar da eşleşir (ör. CatalogDomainException → DomainException).</summary>
    public int GetStatusCode(Exception exception)
    {
        foreach (var (type, statusCode) in _mappings)
        {
            if (type.IsInstanceOfType(exception))
                return statusCode;
        }

        return StatusCodes.Status500InternalServerError;
    }
}
