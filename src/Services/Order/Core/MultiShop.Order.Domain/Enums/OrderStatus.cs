namespace MultiShop.Order.Domain.Enums
{
    // Değerler sipariş saga'sının durumlarıdır; Ordering.Status her geçişte aynı transaction'da güncellenir (KT-12).
    // Sayılar veritabanında saklandığı için sabit: eski Suspend/Completed/Fail değerleri 0/1/2 olarak kaldı.
    public enum OrderStatus
    {
        Pending = 0,          // Sipariş oluşturuldu, stok ayrılması bekleniyor
        Completed = 1,        // Kargo teslim edildi, sipariş tamamlandı (KT-12 öncesi siparişler de bu durumda)
        Cancelled = 2,        // Stok yetersiz, ödeme denemeleri bitti ya da müşteri/admin iptal etti (sebep: FailMessage)
        AwaitingPayment = 3,  // Stok ayrıldı, ödeme bekleniyor
        Paid = 4,             // Ödeme alındı, stok kesinleşti, gönderi hazırlanıyor
        Shipped = 5           // Kargoya verildi (takip no: TrackingNumber)
    }
}
