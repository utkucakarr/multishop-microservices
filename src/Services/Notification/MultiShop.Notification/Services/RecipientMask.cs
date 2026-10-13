using MultiShop.Notification.Data.Entities;

namespace MultiShop.Notification.Services;

/// <summary>Gönderim geçmişinde alıcı adresinin tamamı gösterilmez (KT-13'te müşteri adresleri de burada olacak).</summary>
public static class RecipientMask
{
    public static string Mask(NotificationChannel channel, string address)
    {
        if (channel == NotificationChannel.Email)
        {
            var at = address.IndexOf('@');
            return at > 0 ? $"{address[0]}***{address[at..]}" : "***";
        }

        return address.Length > 4 ? new string('*', address.Length - 4) + address[^4..] : "****";
    }
}
