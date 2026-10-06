namespace MultiShop.Order.Domain.Enums
{
    public enum OrderStatus
    {
        Suspend = 0,    // Sipariş oluşturuldu, ödeme/stok bekleniyor
        Completed = 1,  // Ödeme alındı, stok düşüldü, sipariş onaylandı
        Fail = 2        // Ödeme alınamadı veya stok yetersiz (İptal edildi)
    }
}
