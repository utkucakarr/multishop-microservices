namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>İstek geçersiz (ör. hatalı biçimde id). 400 Bad Request olarak döner.</summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message)
    { }
}
