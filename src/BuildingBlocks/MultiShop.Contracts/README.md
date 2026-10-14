# MultiShop.Contracts

Servislerin RabbitMQ üzerinden birbirine gönderdiği **event ve komut sözleşmeleri**. Yayınlayan ve dinleyen servis bu projeyi referans alır.
Mimari kararlar: `docs/02-mikroservis-bolumleme.md` → "Mesajlaşma mimarisi kararları".

## Kurallar

1. **Hassas veri yok.** Kart numarası, CVV, son kullanma tarihi, şifre, token asla bir event'e eklenmez. Kuyruklar diske ve loglara yazılır, her dinleyen servis içeriği görür.
2. **Event = olmuş bir şey, komut = istenen bir iş.**
   - Event'in adı geçmiş zamanlı, "Event" eki yok: `StockReserved`, `PaymentSucceeded`. `Publish` ile yayınlanır; kim ilgileniyorsa dinler.
   - Komutun adı emir kipinde: `ReserveStock`, `ReleaseStock`. Tek alıcısı vardır, `Send` ile doğrudan onun kuyruğuna gider (orkestrasyon: sipariş saga'sı Catalog'a komut verir).
3. **`record` kullanılır.** Gönderildikten sonra içerik değiştirilemez (immutable).
4. **Bağımlılık yok.** Bu proje hiçbir NuGet paketine ve başka projeye bağlı değil; böylece her servis (ve hangi mesajlaşma kütüphanesi kullanılırsa kullanılsın) referans alabilir.
5. **Geriye dönük uyumluluk.** Yayındaki bir event'e yeni alan eklenebilir; alan silinmez, adı veya tipi değiştirilmez. Büyük değişiklik gerekiyorsa yeni bir event tanımlanır.
6. **Klasör = sahibi olan servis.** Event'lerde yayınlayan (`Orders/` → Order), komutlarda işleyen servis (`Catalog/ReserveStock` → Catalog).
7. **Kullanılmayan event yazılmaz.** Her event, onu yayınlayan ve dinleyen kod yazılırken eklenir.

## Mesajlar

| Mesaj | Tür | Gönderen | Alan | Ne zaman eklendi |
|-------|-----|----------|------|------------------|
| `Baskets/BasketCheckedOut` | Event | Basket | Order (saga) | KT-12 |
| `Catalog/ReserveStock`, `CommitStock`, `ReleaseStock` | Komut | Order (saga) | Catalog | KT-12 |
| `Catalog/StockReserved`, `StockReservationFailed` | Event | Catalog | Order (saga) | KT-12 |
| `Orders/OrderAwaitingPayment`, `OrderCancelled` | Event | Order (saga) | Payment | KT-12 |
| `Payments/PaymentSucceeded`, `PaymentFailed` | Event | Payment | Order (saga) | KT-12 |
| `Comments/CommentCreated` | Event | Comment | Notification | KT-10 |
| `Messages/ContactMessageReceived` | Event | Message | Notification | KT-10 (gönderenin e-postasını taşır, kural 1'e bilinçli istisna: admin mailinde gösteriliyor; mesaj metni yok) |
