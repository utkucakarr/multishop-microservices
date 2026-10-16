using MediatR;
using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.AddressHandlers
{
    // Adresin var olduğu ve kullanıcıya ait olduğu controller'da kontrol edilir (OrderAccessGuard, yoksa 404).
    public class RemoveAddressCommandHandler : IRequestHandler<RemoveAddressCommand>
    {
        private readonly IRepository<Address> _repository;

        public RemoveAddressCommandHandler(IRepository<Address> repository)
        {
            _repository = repository;
        }

        public async Task Handle(RemoveAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _repository.GetByIdAsync(request.Id);
            await _repository.DeleteAsync(address);
        }
    }
}
