namespace MultiShop.Contracts.Payments;

/// <summary>Kart çekimi başarılı. Order saga'sı stoğu kesinleştirir, sipariş Paid olur.</summary>
public record PaymentSucceeded(Guid OrderNumber, decimal Amount, DateTime PaidAt);

/// <summary>Kart çekimi başarısız. Sipariş ödeme beklemeye devam eder; deneme hakkı biterse saga iptal eder.</summary>
/// <param name="Reason">Bankanın kullanıcıya gösterilebilir mesajı.</param>
public record PaymentFailed(Guid OrderNumber, string Reason);
