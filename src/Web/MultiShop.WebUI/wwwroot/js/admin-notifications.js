// Admin paneli canlı bildirimleri: Notification hub'ına gateway üzerinden (WebSocket) bağlanır.
// State + ephemeral event: bildirimler veritabanında kalıcı; push yalnızca "yeni bir şey var" ipucu.
// Bağlanınca ve her yeniden bağlanmada zil sayacı durumdan (REST) yeniden okunur, kaçan push'lar böyle telafi edilir.
// Hub adresi script etiketindeki data-hub-url'den gelir.
(function () {
    var script = document.currentScript;
    var hubUrl = script && script.getAttribute('data-hub-url');
    if (!hubUrl || !window.signalR) {
        return;
    }

    var unreadCount = null;

    function getAccessToken() {
        return fetch('/Admin/Notification/Token', { credentials: 'same-origin', cache: 'no-store' })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Bildirim token\'ı alınamadı (' + response.status + ')');
                }
                return response.json();
            })
            .then(function (data) { return data.accessToken; });
    }

    // skipNegotiation + yalnızca WebSocket: gateway'de tek bir ws rotası yeterli, CORS gerekmez.
    // Yeniden bağlanma hiç vazgeçmez: 0 sn, 2 sn, 10 sn, sonra 30 sn'de bir.
    var connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
            accessTokenFactory: getAccessToken,
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: function (context) {
                return [0, 2000, 10000][context.previousRetryCount] || 30000;
            }
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('ReceiveNotification', function (notification) {
        if (unreadCount !== null) {
            setUnreadCount(unreadCount + 1);
        }
        prependToMenu(notification);
        showToast(notification);
    });
    connection.onreconnected(refreshUnreadCount);

    // withAutomaticReconnect yalnızca kurulmuş bağlantı koparsa devreye girer; ilk bağlantı burada tekrarlanır.
    function start() {
        connection.start()
            .then(refreshUnreadCount)
            .catch(function (error) {
                console.warn('Bildirim bağlantısı kurulamadı, 30 sn sonra tekrar denenecek.', error);
                setTimeout(start, 30000);
            });
    }
    start();

    function refreshUnreadCount() {
        fetch('/Admin/Notification/UnreadCount', { credentials: 'same-origin', cache: 'no-store' })
            .then(function (response) { return response.ok ? response.json() : null; })
            .then(function (count) {
                if (typeof count === 'number') {
                    setUnreadCount(count);
                }
            })
            .catch(function () { /* servis kapalıysa sayaç olduğu gibi kalır */ });
    }

    function setUnreadCount(count) {
        unreadCount = count;
        var badge = document.getElementById('notification-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? '' : 'none';
        }
        var text = document.getElementById('notification-count-text');
        if (text) {
            text.textContent = count;
        }
    }

    function detailUrl(notification) {
        return '/Admin/Notification/Detail/' + encodeURIComponent(notification.id);
    }

    // Metinler textContent ile yazılır: bildirim kullanıcı içeriği (ör. yorum, iletişim konusu) taşıyabilir.
    function prependToMenu(notification) {
        var list = document.getElementById('notification-list');
        if (!list) {
            return;
        }
        var empty = document.getElementById('notification-empty');
        if (empty) {
            empty.remove();
        }

        var item = document.createElement('li');
        var link = document.createElement('a');
        link.href = detailUrl(notification);

        var title = document.createElement('h4');
        title.style.fontWeight = 'bold';
        title.textContent = notification.title;
        link.appendChild(title);

        var message = document.createElement('p');
        message.textContent = notification.message;
        link.appendChild(message);

        item.appendChild(link);
        list.insertBefore(item, list.firstChild);
        while (list.children.length > 5) {
            list.removeChild(list.lastChild);
        }
    }

    function getToastContainer() {
        var container = document.getElementById('admin-notifications');
        if (!container) {
            container = document.createElement('div');
            container.id = 'admin-notifications';
            container.style.cssText = 'position:fixed;top:60px;right:20px;z-index:2000;width:320px;';
            document.body.appendChild(container);
        }
        return container;
    }

    function showToast(notification) {
        var item = document.createElement('div');
        item.className = 'alert alert-info';
        item.style.cssText = 'box-shadow:0 2px 8px rgba(0,0,0,.2);margin-bottom:10px;';

        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'close';
        close.innerHTML = '&times;';
        close.onclick = function () { item.remove(); };
        item.appendChild(close);

        var title = document.createElement('strong');
        title.textContent = notification.title;
        item.appendChild(title);

        var message = document.createElement('div');
        message.textContent = notification.message;
        item.appendChild(message);

        var link = document.createElement('a');
        link.href = detailUrl(notification);
        link.textContent = 'Görüntüle';
        item.appendChild(link);

        getToastContainer().appendChild(item);
        setTimeout(function () { item.remove(); }, 8000);
    }
})();
