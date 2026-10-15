namespace MultiShop.DtoLayer.CargoDtos.ShipmentDtos
{
    /// <summary>Gönderi listesi satırı (Cargo API: <c>GET shipments</c>).</summary>
    public class ResultShipmentDto
    {
        public int CargoDetailId { get; set; }

        public Guid OrderNumber { get; set; }

        public int OrderingId { get; set; }

        public string Barcode { get; set; } = string.Empty;

        /// <summary>Preparing, Shipped, Delivered (<see cref="ShipmentStatuses"/>).</summary>
        public string Status { get; set; } = string.Empty;

        public string ReceiverName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? CargoCompanyName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DispatchedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }
    }
}
