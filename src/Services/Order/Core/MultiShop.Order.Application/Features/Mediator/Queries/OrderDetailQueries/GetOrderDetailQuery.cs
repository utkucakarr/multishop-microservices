using MediatR;
using MultiShop.Order.Application.Features.Mediator.Results.OrderDetailResults;

namespace MultiShop.Order.Application.Features.Mediator.Queries.OrderDetailQueries
{
    /// <summary>Tüm siparişlerin satırları (Admin).</summary>
    public record GetOrderDetailQuery : IRequest<List<GetOrderDetailQueryResult>>;
}
