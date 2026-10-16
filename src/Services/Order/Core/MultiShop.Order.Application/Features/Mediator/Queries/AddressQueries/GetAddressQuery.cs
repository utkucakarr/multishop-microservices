using MediatR;
using MultiShop.Order.Application.Features.Mediator.Results.AddressResults;

namespace MultiShop.Order.Application.Features.Mediator.Queries.AddressQueries
{
    /// <summary>Adresler; <paramref name="UserId"/> verilirse yalnızca o kullanıcınınkiler, <c>null</c> ise hepsi (Admin).</summary>
    public record GetAddressQuery(string? UserId) : IRequest<List<GetAddressQueryResult>>;
}
