using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MultiShop.BuildingBlocks.Authentication;

namespace MultiShop.Notification.Hubs;

/// <summary>
/// Admin paneline anlık bildirim kanalı. Yalnızca sunucu → istemci: istemcinin çağırabileceği metot yok,
/// bildirimler <see cref="Notifications.IAdminNotifier"/> üzerinden gönderilir.
/// </summary>
[Authorize(Policy = MultiShopPolicies.Admin)]
public class NotificationHub : Hub<INotificationClient>
{
    public const string Path = "/hubs/notification";
    public const string AdminsGroup = "admins";

    public override async Task OnConnectedAsync()
    {
        // Hub yalnızca Admin'e açık; bağlanan her bağlantı admin grubuna girer. Kullanıcıya özel
        // bildirimler (KT-13) için ayrıca Clients.User(sub) kullanılabilir.
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
        await base.OnConnectedAsync();
    }
}
