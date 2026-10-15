using MultiShop.Order.Application.Features.CQRS.Commands.AddressCommands;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.CQRS.Handlers.AddressHandlers
{
    public class CreateAddressCommandHandler
    {
        private readonly IRepository<Address> _repository;

        public CreateAddressCommandHandler(IRepository<Address> repository)
        {
            _repository = repository;
        }
        /// <returns>Oluşturulan adresin id'si (sepet onayında teslimat adresi olarak gönderilir).</returns>
        public async Task<int> Handle(CreateAddressCommand createAddressCommand)
        {
            var address = await _repository.CreateAsync(new Address
            {
                City = createAddressCommand.City,
                Detail1 = createAddressCommand.Detail1,
                District = createAddressCommand.District,
                UserId = createAddressCommand.UserId!,
                Country = createAddressCommand.Country,
                Description = createAddressCommand.Description ?? string.Empty,
                Detail2 = createAddressCommand.Detail2 ?? string.Empty,
                Email = createAddressCommand.Email,
                Name = createAddressCommand.Name,
                Phone = createAddressCommand.Phone,
                SurName = createAddressCommand.SurName,
                ZipCode = createAddressCommand.ZipCode ?? string.Empty
            });
            return address.AddressId;
        }
    }
}
