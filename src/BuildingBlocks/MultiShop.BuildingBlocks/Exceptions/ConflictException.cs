namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>İstek mevcut veriyle çakışıyor (ör. aynı kupon kodu zaten var). 409 Conflict olarak döner.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
