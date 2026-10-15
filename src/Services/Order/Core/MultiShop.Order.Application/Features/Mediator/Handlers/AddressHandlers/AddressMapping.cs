using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;
using MultiShop.Order.Application.Features.Mediator.Results.AddressResults;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.AddressHandlers
{
    internal static class AddressMapping
    {
        /// <summary>Komuttaki alanları adrese yazar; isteğe bağlı alanlar boşsa "" (sütunlar NOT NULL).</summary>
        public static void Apply(this Address address, AddressCommandBase command)
        {
            address.UserId = command.UserId;
            address.Name = command.Name;
            address.SurName = command.SurName;
            address.Email = command.Email;
            address.Phone = command.Phone;
            address.Country = command.Country;
            address.District = command.District;
            address.City = command.City;
            address.Detail1 = command.Detail1;
            address.Detail2 = command.Detail2 ?? string.Empty;
            address.Description = command.Description ?? string.Empty;
            address.ZipCode = command.ZipCode ?? string.Empty;
        }

        public static GetAddressQueryResult ToResult(this Address address) => new()
        {
            AddressId = address.AddressId,
            UserId = address.UserId,
            Name = address.Name,
            SurName = address.SurName,
            Email = address.Email,
            Phone = address.Phone,
            Country = address.Country,
            District = address.District,
            City = address.City,
            Detail1 = address.Detail1,
            Detail2 = address.Detail2,
            Description = address.Description,
            ZipCode = address.ZipCode
        };
    }
}
