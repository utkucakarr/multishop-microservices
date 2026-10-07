namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>İstenen kayıt yok. 404 Not Found olarak döner.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    { }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} bulunamadı: {key}")
    { }
}
