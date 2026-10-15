namespace MultiShop.DtoLayer.CargoDtos.ShipmentDtos
{
    /// <summary>Gönderi, alıcı adresi ve hareket geçmişi (Cargo API: <c>GET shipments/{id}</c>).</summary>
    public class GetShipmentByIdDto
    {
        public int CargoDetailId { get; set; }

        public Guid OrderNumber { get; set; }

        public int OrderingId { get; set; }

        public string Barcode { get; set; } = string.Empty;

        /// <summary>Preparing, Shipped, Delivered (<see cref="ShipmentStatuses"/>).</summary>
        public string Status { get; set; } = string.Empty;

        public string ReceiverName { get; set; } = string.Empty;

        public string ReceiverEmail { get; set; } = string.Empty;

        public string ReceiverPhone { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string? ZipCode { get; set; }

        public string? CargoCompanyName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DispatchedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public List<ShipmentOperationDto> Operations { get; set; } = new();
    }

    public class ShipmentOperationDto
    {
        public string Description { get; set; } = string.Empty;

        public DateTime OperationDate { get; set; }
    }
}
