namespace MultiShop.Notification.Dispatching;

/// <summary>İlk deneme + 3 tekrar (1, 5, 15 dk); sonra kalıcı Failed ve panelden elle tekrar gönderme.</summary>
public static class RetryPolicy
{
    private static readonly TimeSpan[] Delays = { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15) };

    public static int MaxAttempts => Delays.Length + 1;

    /// <summary><paramref name="attempts"/> deneme başarısız olduysa bir sonrakine kadar beklenecek süre; deneme hakkı bittiyse null.</summary>
    public static TimeSpan? GetRetryDelay(int attempts)
        => attempts >= 1 && attempts <= Delays.Length ? Delays[attempts - 1] : null;
}
