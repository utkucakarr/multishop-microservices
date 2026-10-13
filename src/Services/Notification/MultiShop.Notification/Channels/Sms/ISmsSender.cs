namespace MultiShop.Notification.Channels.Sms;

public interface ISmsSender
{
    /// <summary>Başarısızlıkta exception fırlatır.</summary>
    Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

public enum SmsSendResult
{
    Sent,
    /// <summary>Gerçek sağlayıcı yok; gönderilmedi.</summary>
    Simulated,
}
