using MultiShop.Cargo.EntityLayer.Concrete;

namespace MultiShop.Cargo.WebApi.Shipments
{
    public record ShipmentListItemDto(
        int CargoDetailId,
        Guid OrderNumber,
        int OrderingId,
        string Barcode,
        ShipmentStatus Status,
        string ReceiverName,
        string City,
        string? CargoCompanyName,
        DateTime CreatedAt,
        DateTime? DispatchedAt,
        DateTime? DeliveredAt);

    public record ShipmentOperationDto(string Description, DateTime OperationDate);

    public record ShipmentDetailDto(
        int CargoDetailId,
        Guid OrderNumber,
        int OrderingId,
        string Barcode,
        ShipmentStatus Status,
        string ReceiverName,
        string ReceiverEmail,
        string ReceiverPhone,
        string AddressLine1,
        string? AddressLine2,
        string District,
        string City,
        string Country,
        string? ZipCode,
        string? CargoCompanyName,
        DateTime CreatedAt,
        DateTime? DispatchedAt,
        DateTime? DeliveredAt,
        IReadOnlyList<ShipmentOperationDto> Operations);
}
