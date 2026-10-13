namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>İşin bağlı olduğu başka bir servise ulaşılamadı (ör. Basket → Catalog). 503 Service Unavailable olarak döner.</summary>
public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message)
        : base(message)
    { }

    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
