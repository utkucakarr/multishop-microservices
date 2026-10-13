# MultiShop.Contracts

Servislerin RabbitMQ üzerinden birbirine gönderdiği **event sözleşmeleri**. Yayınlayan ve dinleyen servis bu projeyi referans alır.
Mimari kararlar: `docs/02-mikroservis-bolumleme.md` → "Mesajlaşma mimarisi kararları".

## Kurallar

1. **Hassas veri yok.** Kart numarası, CVV, son kullanma tarihi, şifre, token asla bir event'e eklenmez. Kuyruklar diske ve loglara yazılır, her dinleyen servis içeriği görür.
2. **Ad geçmiş zamanlı, "Event" eki yok:** `OrderCreated`, `StockReserved`, `PaymentSucceeded`. Event olmuş bir şeyi duyurur, komut vermez.
3. **`record` kullanılır.** Gönderildikten sonra içerik değiştirilemez (immutable).
4. **Bağımlılık yok.** Bu proje hiçbir NuGet paketine ve başka projeye bağlı değil; böylece her servis (ve hangi mesajlaşma kütüphanesi kullanılırsa kullanılsın) referans alabilir.
5. **Geriye dönük uyumluluk.** Yayındaki bir event'e yeni alan eklenebilir; alan silinmez, adı veya tipi değiştirilmez. Büyük değişiklik gerekiyorsa yeni bir event tanımlanır.
6. **Klasör = sahibi olan servis.** `Orders/` altındakileri Order yayınlar, `Catalog/` altındakileri Catalog, vb.
7. **Kullanılmayan event yazılmaz.** Her event, onu yayınlayan ve dinleyen kod yazılırken eklenir.

## Event'ler

| Event | Yayınlayan | Dinleyen | Ne zaman eklendi |
|-------|-----------|----------|------------------|
| `Orders/OrderCreated` | Order | Catalog | KT-9 (sözleşme); kullanım KT-12 |
| `Comments/CommentCreated` | Comment | Notification | KT-10 |
| `Messages/ContactMessageReceived` | Message | Notification | KT-10 (gönderenin e-postasını taşır, kural 1'e bilinçli istisna: admin mailinde gösteriliyor; mesaj metni yok) |
