namespace MultiShop.Contracts.Payments;

/// <summary>
/// Ödeme kuralları; Payment uygular, Order saga'sı aynı sınırla siparişi iptal eder. İki serviste ayrı sabit
/// tutulursa biri değiştiğinde diğeri eskide kalır (ör. Payment 4. denemeyi kabul eder, saga 3.'de iptal etmiştir).
/// </summary>
public static class PaymentPolicy
{
    /// <summary>Bir sipariş için en fazla bu kadar başarısız deneme; sonra ödeme kapanır ve sipariş iptal edilir.</summary>
    public const int MaxAttempts = 3;
}
