using MediatR;
using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.AddressHandlers
{
    // Adresin var olduğu ve kullanıcıya ait olduğu controller'da kontrol edilir (OrderAccessGuard, yoksa 404).
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand>
    {
        private readonly IRepository<Address> _repository;

        public UpdateAddressCommandHandler(IRepository<Address> repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _repository.GetByIdAsync(request.AddressId);
            address.Apply(request);
            await _repository.UpdateAsync(address);
        }
    }
}
