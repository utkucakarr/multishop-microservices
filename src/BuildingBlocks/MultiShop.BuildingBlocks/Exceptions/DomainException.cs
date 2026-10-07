namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>
/// İş kuralı ihlali. İstemci hatası sayılır ve 400 Bad Request olarak döner.
/// Servisler kendi domain exception'larını bu sınıftan türetir.
/// </summary>
public class DomainException : Exception
{
    public DomainException()
    { }

    public DomainException(string message)
        : base(message)
    { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
