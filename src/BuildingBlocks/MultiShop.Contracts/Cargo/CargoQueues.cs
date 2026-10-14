namespace MultiShop.Contracts.Cargo;

/// <summary>Cargo'nun komut kuyrukları. Gönderen (Order saga'sı) <c>Send</c> ile doğrudan bu kuyruğa yazar.</summary>
public static class CargoQueues
{
    public const string CreateShipment = "cargo-create-shipment";
}
