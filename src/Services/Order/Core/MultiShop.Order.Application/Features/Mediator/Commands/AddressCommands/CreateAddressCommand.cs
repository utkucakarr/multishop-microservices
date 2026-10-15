using MediatR;

namespace MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands
{
    /// <summary>Yeni adres; oluşturulan adresin id'sini döner (sepet onayında teslimat adresi olarak gönderilir).</summary>
    public class CreateAddressCommand : AddressCommandBase, IRequest<int>
    {
    }
}
