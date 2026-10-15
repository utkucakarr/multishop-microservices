using MultiShop.DtoLayer.CargoDtos.ShipmentDtos;

namespace MultiShop.WebUI.Services.CargoServices.ShipmentServices
{
    /// <summary>
    /// Cargo servisinin gönderi istemcisi. Gönderi WebUI'dan açılmaz: ödenen siparişe saga'nın komutuyla Cargo açar;
    /// burada listelenir, kargoya verilir ve teslim edildi işaretlenir.
    /// </summary>
    public interface IShipmentService
    {
        /// <summary>Gönderiler (en yeni önce); <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        Task<List<ResultShipmentDto>> GetAllAsync(string? status);

        /// <summary>Gönderi, alıcı adresi ve hareket geçmişi; yoksa null.</summary>
        Task<GetShipmentByIdDto?> GetByIdAsync(int id);

        /// <summary>Kargoya verir (sipariş "Kargoda" olur). Başarıda null, değilse Cargo'nun mesajı.</summary>
        Task<string?> DispatchAsync(int id);

        /// <summary>Teslim edildi işaretler (sipariş tamamlanır). Başarıda null, değilse Cargo'nun mesajı.</summary>
        Task<string?> DeliverAsync(int id);
    }
}
