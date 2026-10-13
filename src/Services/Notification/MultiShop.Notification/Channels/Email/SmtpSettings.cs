namespace MultiShop.Notification.Channels.Email;

/// <summary><c>SmtpSettings</c> ayar bölümü (WebUI ile aynı ad). <c>UserName</c> ve <c>Password</c> user-secrets'ta.</summary>
public class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string SenderName { get; set; } = "MultiShop";
    public string? SenderEmail { get; set; }
}
