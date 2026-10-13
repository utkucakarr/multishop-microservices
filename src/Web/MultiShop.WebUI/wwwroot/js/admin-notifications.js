// Admin paneli canlı bildirimleri: Notification hub'ına gateway üzerinden (WebSocket) bağlanır,
// gelen bildirimi sağ üstte birkaç saniye gösterir. Hub adresi script etiketindeki data-hub-url'den gelir.
(function () {
    var script = document.currentScript;
    var hubUrl = script && script.getAttribute('data-hub-url');
    if (!hubUrl || !window.signalR) {
        return;
    }

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
    var connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
            accessTokenFactory: getAccessToken,
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('ReceiveNotification', showNotification);

    // withAutomaticReconnect yalnızca kurulmuş bağlantı koparsa devreye girer; ilk bağlantı denemesi burada tekrarlanır.
    function start() {
        connection.start().catch(function (error) {
            console.warn('Bildirim bağlantısı kurulamadı, 30 sn sonra tekrar denenecek.', error);
            setTimeout(start, 30000);
        });
    }
    start();

    function getContainer() {
        var container = document.getElementById('admin-notifications');
        if (!container) {
            container = document.createElement('div');
            container.id = 'admin-notifications';
            container.style.cssText = 'position:fixed;top:60px;right:20px;z-index:2000;width:320px;';
            document.body.appendChild(container);
        }
        return container;
    }

    function showNotification(notification) {
        var item = document.createElement('div');
        item.className = 'alert alert-info';
        item.style.cssText = 'box-shadow:0 2px 8px rgba(0,0,0,.2);margin-bottom:10px;';

        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'close';
        close.innerHTML = '&times;';
        close.onclick = function () { item.remove(); };
        item.appendChild(close);

        // Metinler textContent ile yazılır: bildirim ileride kullanıcı içeriği (ör. yorum) taşıyacak, HTML olarak işlenmemeli.
        var title = document.createElement('strong');
        title.textContent = notification.title;
        item.appendChild(title);

        var message = document.createElement('div');
        message.textContent = notification.message;
        item.appendChild(message);

        // Yalnızca site içi bağlantı (ör. /Admin/Comment/Index); javascript: gibi adresler kabul edilmez.
        if (notification.link && notification.link.charAt(0) === '/' && notification.link.charAt(1) !== '/') {
            var link = document.createElement('a');
            link.href = notification.link;
            link.textContent = 'Görüntüle';
            item.appendChild(link);
        }

        getContainer().appendChild(item);
        setTimeout(function () { item.remove(); }, 8000);
    }
})();
