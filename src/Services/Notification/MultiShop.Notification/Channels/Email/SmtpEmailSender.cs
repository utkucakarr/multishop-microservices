using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MultiShop.Notification.Channels.Email;

/// <summary>MailKit ile SMTP üzerinden gönderim. Her gönderimde bağlanıp kapatır (düşük hacim için yeterli).</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        // Geliştirmede SMTP hesabı tanımlı olmayabilir; uygulama açılışta değil, ilk gönderimde hata verir.
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
            throw new InvalidOperationException("SmtpSettings:Host ve SmtpSettings:SenderEmail ayarları eksik.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = isHtml ? body : null, TextBody = isHtml ? null : body }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.Auto, cancellationToken);
        if (!string.IsNullOrEmpty(_settings.UserName))
            await client.AuthenticateAsync(_settings.UserName, _settings.Password ?? string.Empty, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Mail gönderildi: {Subject}", subject);
    }
}
