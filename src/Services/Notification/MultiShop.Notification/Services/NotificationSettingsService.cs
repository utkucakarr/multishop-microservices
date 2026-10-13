using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Notification.Channels.Email;
using MultiShop.Notification.Data;
using MultiShop.Notification.Data.Entities;
using MultiShop.Notification.Dtos;

namespace MultiShop.Notification.Services;

public partial class NotificationSettingsService : INotificationSettingsService
{
    private readonly NotificationDbContext _db;
    private readonly SmtpSettings _smtpSettings;

    public NotificationSettingsService(NotificationDbContext db, IOptions<SmtpSettings> smtpSettings)
    {
        _db = db;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task<NotificationSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        var rules = await _db.Rules.AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .Select(x => new NotificationRuleDto(x.Type, x.DisplayName, x.InAppEnabled, x.EmailEnabled, x.SmsEnabled))
            .ToListAsync(cancellationToken);
        var recipients = await _db.Recipients.AsNoTracking()
            .OrderBy(x => x.Channel).ThenBy(x => x.Address)
            .Select(x => new NotificationRecipientDto(x.Id, x.Channel, x.Address, x.Name, x.IsActive))
            .ToListAsync(cancellationToken);

        return new NotificationSettingsDto(rules, recipients, BuildWarnings(rules, recipients));
    }

    // Kural açık ama gönderim yapılamayacak durumlar; panelde sarı uyarı.
    private List<string> BuildWarnings(List<NotificationRuleDto> rules, List<NotificationRecipientDto> recipients)
    {
        var warnings = new List<string>();
        var activeChannels = recipients.Where(x => x.IsActive).Select(x => x.Channel).ToHashSet();

        if (rules.Any(x => x.EmailEnabled))
        {
            if (!activeChannels.Contains(NotificationChannel.Email))
                warnings.Add("Mail kanalı bazı bildirimlerde açık ama aktif mail alıcısı yok; mail gönderilmeyecek.");
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host) || string.IsNullOrWhiteSpace(_smtpSettings.SenderEmail))
                warnings.Add("SMTP ayarları eksik (SmtpSettings); mailler gönderilemeyecek ve başarısız olarak görünecek.");
        }

        if (rules.Any(x => x.SmsEnabled))
        {
            if (!activeChannels.Contains(NotificationChannel.Sms))
                warnings.Add("SMS kanalı bazı bildirimlerde açık ama aktif telefon alıcısı yok; SMS gönderilmeyecek.");
            warnings.Add("SMS sağlayıcısı henüz tanımlı değil; SMS'ler gönderilmez, \"Simulated\" olarak kaydedilir.");
        }

        return warnings;
    }

    public async Task UpdateRulesAsync(IReadOnlyCollection<UpdateNotificationRuleDto> rules, CancellationToken cancellationToken)
    {
        var existing = await _db.Rules.ToDictionaryAsync(x => x.Type, cancellationToken);
        foreach (var update in rules)
        {
            if (!existing.TryGetValue(update.Type, out var rule))
                throw new BadRequestException($"Bilinmeyen bildirim türü: {update.Type}");

            rule.InAppEnabled = update.InAppEnabled;
            rule.EmailEnabled = update.EmailEnabled;
            rule.SmsEnabled = update.SmsEnabled;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationRecipientDto> AddRecipientAsync(CreateNotificationRecipientDto recipient, CancellationToken cancellationToken)
    {
        var address = NormalizeAddress(recipient.Channel, recipient.Address);
        var name = string.IsNullOrWhiteSpace(recipient.Name) ? null : recipient.Name.Trim();
        if (name is { Length: > NotificationLimits.RecipientName })
            throw new BadRequestException($"Ad en fazla {NotificationLimits.RecipientName} karakter olabilir.");

        if (await _db.Recipients.AnyAsync(x => x.Channel == recipient.Channel && x.Address == address, cancellationToken))
            throw new BadRequestException("Bu alıcı zaten ekli.");

        var entity = new NotificationRecipient
        {
            Id = Guid.NewGuid(),
            Channel = recipient.Channel,
            Address = address,
            Name = name,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Recipients.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new NotificationRecipientDto(entity.Id, entity.Channel, entity.Address, entity.Name, entity.IsActive);
    }

    public async Task SetRecipientActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var recipient = await _db.Recipients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Alıcı", id);
        recipient.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRecipientAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _db.Recipients.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
            throw new NotFoundException("Alıcı", id);
    }

    private static string NormalizeAddress(NotificationChannel channel, string? address)
    {
        var value = address?.Trim() ?? string.Empty;
        switch (channel)
        {
            case NotificationChannel.Email:
                if (value.Length > NotificationLimits.Address || !MailAddress.TryCreate(value, out var mail) || mail.Address != value)
                    throw new BadRequestException("Geçerli bir mail adresi girin.");
                return value.ToLowerInvariant();

            case NotificationChannel.Sms:
                // Boşluk, tire ve parantezler atılır; uluslararası biçim beklenir (ör. +905xxxxxxxxx).
                var phone = PhoneSeparators().Replace(value, string.Empty);
                if (!PhoneRegex().IsMatch(phone))
                    throw new BadRequestException("Geçerli bir telefon numarası girin (ör. +905xxxxxxxxx).");
                return phone;

            default:
                throw new BadRequestException("Alıcı yalnızca Mail ya da SMS kanalı için eklenebilir.");
        }
    }

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex PhoneSeparators();

    [GeneratedRegex(@"^\+?[1-9][0-9]{9,14}$")]
    private static partial Regex PhoneRegex();
}
