namespace MultiShop.Notification.Data.Entities;

/// <summary>Alan uzunlukları; hem veritabanı kolonları hem istek doğrulaması bunları kullanır.</summary>
public static class NotificationLimits
{
    public const int Type = 100;
    public const int Title = 200;
    public const int Message = 2000;
    public const int Link = 500;
    public const int UserId = 100;
    public const int UserName = 100;
    public const int Address = 254;
    public const int RecipientName = 100;
    public const int Subject = 200;
    public const int Body = 4000;
    public const int Error = 1000;
    public const int DisplayName = 100;
}
