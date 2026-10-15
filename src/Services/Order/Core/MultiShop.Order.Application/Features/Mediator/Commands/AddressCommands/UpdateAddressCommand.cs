using MediatR;

namespace MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands
{
    /// <summary>Adresin tüm alanlarını günceller (önceden 12 alanın yalnızca 4'ü yazılıyordu, ORD-04).</summary>
    public class UpdateAddressCommand : AddressCommandBase, IRequest
    {
        public int AddressId { get; set; }
    }
}
