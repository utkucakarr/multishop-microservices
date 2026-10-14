using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderOrderingServices
{
    /// <summary>
    /// Order servisinin sipariş istemcisi. Sipariş WebUI'dan açılmaz: sepet onayı (Basket checkout) siparişi başlatır,
    /// durumu Order'daki saga yönetir; burada yalnızca okunur ve iptal istenir.
    /// </summary>
    public interface IOrderOrderingService
    {
        /// <summary>Giriş yapmış kullanıcının kendi siparişleri; kullanıcı Order servisinde token'dan belirlenir.</summary>
        Task<List<ResultOrderingByUserIdDto>> GetMyOrderingsAsync();

        /// <summary>Sipariş numarasıyla sipariş ve durumu; henüz oluşmadıysa ya da kullanıcıya ait değilse null.</summary>
        Task<ResultOrderingByUserIdDto?> GetByNumberAsync(Guid orderNumber);

        /// <summary>Ödenmemiş siparişi iptal ettirir (sahibi ya da Admin). Başarıda null, değilse Order'ın mesajı.</summary>
        Task<string?> CancelAsync(Guid orderNumber);

        /// <summary>Tüm siparişler (Admin); <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        Task<List<ResultOrderingByUserIdDto>> GetAllAsync(string? status);
    }
}
