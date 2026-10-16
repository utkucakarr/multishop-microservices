using MediatR;
using MultiShop.Order.Application.Features.Mediator.Results.OrderDetailResults;

namespace MultiShop.Order.Application.Features.Mediator.Queries.OrderDetailQueries
{
    /// <summary>Bir siparişin satırları.</summary>
    public record GetOrderDetailByOrderingIdQuery(int OrderingId) : IRequest<List<GetOrderDetailQueryResult>>;
}
