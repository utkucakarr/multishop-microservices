using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Models
{
    /// <summary>Ödeme sayfası: siparişin durumuna göre bekleme ekranı, kart formu ya da iptal sebebi gösterilir.</summary>
    public class PaymentPageViewModel
    {
        public Guid OrderNumber { get; set; }

        /// <summary>Sipariş henüz oluşmadıysa (saga sepet onayını işliyor) null.</summary>
        public ResultOrderingByUserIdDto? Order { get; set; }

        public string Status => Order?.Status ?? OrderStatusDisplay.Pending;

        /// <summary>İptal istendi, saga henüz işlemedi.</summary>
        public bool Cancelling { get; set; }

        /// <summary>Bekleme ekranı: sipariş hazırlanıyor (stok ayrılıyor) ya da iptal ediliyor; durum değişince sayfa yenilenir.</summary>
        public bool IsWaiting => Status == OrderStatusDisplay.Pending || Cancelling;
    }
}
