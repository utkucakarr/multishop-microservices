namespace MultiShop.Order.Application.Sagas
{
    /// <summary>
    /// Müşteri ya da admin siparişi iptal etti. Order'ın kendi saga'sına gönderdiği iç komuttur, başka servis
    /// kullanmadığı için Contracts'ta değildir. API iptal edilebilirliği önceden kontrol eder; saga yine de yalnızca
    /// Pending ve AwaitingPayment durumlarında iptal eder (arada ödeme gelmiş olabilir).
    /// </summary>
    public record CancelOrder(Guid OrderNumber, string Reason);
}
