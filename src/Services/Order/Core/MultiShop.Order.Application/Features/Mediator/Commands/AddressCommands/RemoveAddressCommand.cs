using MediatR;

namespace MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands
{
    public record RemoveAddressCommand(int Id) : IRequest;
}
