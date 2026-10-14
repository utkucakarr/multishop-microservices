namespace MultiShop.Cargo.EntityLayer.Concrete
{
    /// <summary>Gönderinin hareket geçmişindeki bir satır (hazırlanıyor, kargoya verildi, teslim edildi).</summary>
    public class CargoOperation
    {
        public int CargoOperationId { get; set; }

        /// <summary>Hareketin ait olduğu gönderi. Eskiden yalnızca serbest metin barkodla eşleşiyordu (CAR-01); eşleşmeyen eski kayıtlarda boş.</summary>
        public int? CargoDetailId { get; set; }

        public CargoDetail? CargoDetail { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime OperationDate { get; set; }
    }
}
