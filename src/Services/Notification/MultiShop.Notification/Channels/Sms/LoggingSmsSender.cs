namespace MultiShop.Notification.Channels.Sms;

/// <summary>
/// Gerçek sağlayıcı (ör. Netgsm, Twilio) KT-13'te eklenecek. O zamana kadar SMS gönderilmez, yalnızca loglanır;
/// telefon numarasının yalnızca son 4 hanesi yazılır.
/// </summary>
public class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var masked = phoneNumber.Length > 4 ? new string('*', phoneNumber.Length - 4) + phoneNumber[^4..] : "****";
        _logger.LogWarning("SMS sağlayıcısı yok, gönderilmedi. Alıcı: {PhoneNumber}, mesaj: {SmsMessage}", masked, message);
        return Task.CompletedTask;
    }
}
