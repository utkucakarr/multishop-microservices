using System.Globalization;
using MassTransit;
using MultiShop.Contracts.Messages;
using MultiShop.Notification.Data;
using MultiShop.Notification.Dispatching;

namespace MultiShop.Notification.Consumers;

/// <summary>
/// İletişim formundan yeni mesaj → admin'lere bildirim (kanallar "İletişim formu mesajı" kuralından; varsayılan panel içi + mail).
/// Kuyruk: <c>notification-contact-message-received</c>. Inbox sayesinde aynı event iki kez gelse de bildirim bir kez oluşur.
/// </summary>
public class ContactMessageReceivedConsumer : IConsumer<ContactMessageReceived>
{
    private readonly INotificationDispatcher _dispatcher;

    public ContactMessageReceivedConsumer(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<ContactMessageReceived> context)
    {
        var message = context.Message;
        var request = new NotificationRequest(
            NotificationTypes.ContactMessageReceived,
            NotificationTarget.Admins,
            new Dictionary<string, string>
            {
                ["ContactMessageId"] = message.ContactMessageId.ToString(CultureInfo.InvariantCulture),
                ["Name"] = message.NameSurname,
                ["Email"] = message.Email,
                ["Subject"] = message.Subject,
            });

        await _dispatcher.DispatchAsync(request, context.CancellationToken);
    }
}
