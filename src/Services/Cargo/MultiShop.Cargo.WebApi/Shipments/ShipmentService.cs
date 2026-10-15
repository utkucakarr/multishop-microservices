using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Cargo.DataAccessLayer.Concrete;
using MultiShop.Cargo.EntityLayer.Concrete;
using MultiShop.Contracts.Cargo;

namespace MultiShop.Cargo.WebApi.Shipments
{
    /// <summary>
    /// Gönderiler (KT-13): Order saga'sının komutuyla açılır, admin panelinden kargoya verilir ve teslim edilir. Durum
    /// değişiklikleri event olarak yayınlanır (outbox, aynı transaction): Order siparişi ilerletir, Notification müşteriye yazar.
    /// </summary>
    public class ShipmentService
    {
        private readonly CargoContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(CargoContext context, IPublishEndpoint publishEndpoint, ILogger<ShipmentService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        /// <summary>Siparişin gönderisini açar; aynı sipariş için gönderi varsa bir şey yapmaz (komut tekrar gelebilir).</summary>
        public async Task CreateAsync(CreateShipment command, CancellationToken cancellationToken = default)
        {
            if (await _context.CargoDetails.AnyAsync(x => x.OrderNumber == command.OrderNumber, cancellationToken))
                return;

            var company = await GetDefaultCompanyAsync(cancellationToken);
            if (company is null)
                _logger.LogWarning("Sipariş {OrderNumber} için gönderi firmasız açıldı: hiç kargo firması yok.", command.OrderNumber);

            var recipient = command.Recipient;
            var now = DateTime.UtcNow;
            var shipment = new CargoDetail
            {
                OrderNumber = command.OrderNumber,
                OrderingId = command.OrderingId,
                UserId = command.UserId,
                ReceiverName = $"{recipient.Name} {recipient.Surname}".Trim(),
                ReceiverEmail = recipient.Email,
                ReceiverPhone = recipient.Phone,
                AddressLine1 = recipient.AddressLine1,
                AddressLine2 = recipient.AddressLine2,
                District = recipient.District,
                City = recipient.City,
                Country = recipient.Country,
                ZipCode = recipient.ZipCode,
                // Sipariş numarasından: tekrar gelen komutta aynı barkod, eski sayısal barkodlarla çakışmaz.
                Barcode = $"MS{command.OrderingId:D8}",
                Status = ShipmentStatus.Preparing,
                CargoCompanyId = company?.CargoCompanyId,
                CreatedAt = now
            };
            shipment.Operations.Add(new CargoOperation { Barcode = shipment.Barcode, Description = "Sipariş alındı, gönderi hazırlanıyor.", OperationDate = now });

            _context.CargoDetails.Add(shipment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ShipmentListItemDto>> GetAllAsync(ShipmentStatus? status)
        {
            var query = _context.CargoDetails.AsNoTracking();
            if (status is not null)
                query = query.Where(x => x.Status == status);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ShipmentListItemDto(x.CargoDetailId, x.OrderNumber, x.OrderingId, x.Barcode, x.Status, x.ReceiverName, x.City,
                    x.CargoCompany != null ? x.CargoCompany.CargoCompanyName : null, x.CreatedAt, x.DispatchedAt, x.DeliveredAt))
                .ToListAsync();
        }

        public async Task<ShipmentDetailDto> GetAsync(int id)
        {
            var x = await _context.CargoDetails.AsNoTracking()
                .Include(s => s.CargoCompany)
                .Include(s => s.Operations)
                .SingleOrDefaultAsync(s => s.CargoDetailId == id)
                ?? throw new NotFoundException("Gönderi", id);

            return new ShipmentDetailDto(x.CargoDetailId, x.OrderNumber, x.OrderingId, x.Barcode, x.Status, x.ReceiverName, x.ReceiverEmail,
                x.ReceiverPhone, x.AddressLine1, x.AddressLine2, x.District, x.City, x.Country, x.ZipCode, x.CargoCompany?.CargoCompanyName,
                x.CreatedAt, x.DispatchedAt, x.DeliveredAt,
                x.Operations.OrderBy(o => o.OperationDate).Select(o => new ShipmentOperationDto(o.Description, o.OperationDate)).ToList());
        }

        /// <summary>Hazırlanan gönderiyi kargo firmasına verir; firma yoksa varsayılan atanır, hiç firma yoksa 409.</summary>
        public async Task DispatchAsync(int id)
        {
            var shipment = await GetForUpdateAsync(id);
            if (shipment.Status != ShipmentStatus.Preparing)
                throw new ConflictException("Yalnızca hazırlanan gönderi kargoya verilebilir.");

            var company = shipment.CargoCompany ?? await GetDefaultCompanyAsync()
                ?? throw new ConflictException("Kargo firması yok. Önce Kargo sayfasından bir firma ekleyin.");

            var now = DateTime.UtcNow;
            await ChangeStatusAsync(shipment, ShipmentStatus.Preparing, ShipmentStatus.Shipped, now,
                set => set
                    .SetProperty(x => x.CargoCompanyId, company.CargoCompanyId)
                    .SetProperty(x => x.DispatchedAt, now),
                $"Kargoya verildi ({company.CargoCompanyName}).",
                new ShipmentDispatched(shipment.OrderNumber, shipment.Barcode, company.CargoCompanyName,
                    shipment.UserId, shipment.ReceiverName, shipment.ReceiverEmail, now));
        }

        /// <summary>Kargodaki gönderiyi teslim edildi yapar; sipariş tamamlanır.</summary>
        public async Task DeliverAsync(int id)
        {
            var shipment = await GetForUpdateAsync(id);
            if (shipment.Status != ShipmentStatus.Shipped)
                throw new ConflictException("Yalnızca kargodaki gönderi teslim edildi yapılabilir.");

            var now = DateTime.UtcNow;
            await ChangeStatusAsync(shipment, ShipmentStatus.Shipped, ShipmentStatus.Delivered, now,
                set => set.SetProperty(x => x.DeliveredAt, now),
                "Teslim edildi.",
                new ShipmentDelivered(shipment.OrderNumber, shipment.Barcode, now));
        }

        // Koşullu UPDATE (yalnızca beklenen durumdaysa): iki admin aynı anda tıklarsa ikincisi 409 alır, event ve müşteri
        // maili bir kez gider. Durum, hareket satırı ve event (outbox) tek transaction'da.
        private async Task ChangeStatusAsync<TEvent>(CargoDetail shipment, ShipmentStatus from, ShipmentStatus to, DateTime now,
            Func<Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<CargoDetail>, Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<CargoDetail>> extra,
            string operation, TEvent @event) where TEvent : class
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var updated = await _context.CargoDetails
                .Where(x => x.CargoDetailId == shipment.CargoDetailId && x.Status == from)
                .ExecuteUpdateAsync(set => extra(set.SetProperty(x => x.Status, to)));
            if (updated == 0)
                throw new ConflictException("Gönderinin durumu bu arada değişti; sayfayı yenileyin.");

            _context.CargoOperations.Add(new CargoOperation
            {
                CargoDetailId = shipment.CargoDetailId,
                Barcode = shipment.Barcode,
                Description = operation,
                OperationDate = now
            });
            await _publishEndpoint.Publish(@event);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task<CargoDetail> GetForUpdateAsync(int id)
            => await _context.CargoDetails.AsNoTracking().Include(x => x.CargoCompany).SingleOrDefaultAsync(x => x.CargoDetailId == id)
                ?? throw new NotFoundException("Gönderi", id);

        // Varsayılan işaretli firma; yoksa ilk eklenen.
        private Task<CargoCompany?> GetDefaultCompanyAsync(CancellationToken cancellationToken = default)
            => _context.CargoCompanies.AsNoTracking()
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.CargoCompanyId)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
