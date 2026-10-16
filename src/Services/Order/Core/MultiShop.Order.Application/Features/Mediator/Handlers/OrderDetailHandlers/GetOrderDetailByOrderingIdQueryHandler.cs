using MediatR;
using MultiShop.Order.Application.Features.Mediator.Queries.OrderDetailQueries;
using MultiShop.Order.Application.Features.Mediator.Results.OrderDetailResults;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.OrderDetailHandlers
{
    public class GetOrderDetailByOrderingIdQueryHandler : IRequestHandler<GetOrderDetailByOrderingIdQuery, List<GetOrderDetailQueryResult>>
    {
        private readonly IRepository<OrderDetail> _repository;

        public GetOrderDetailByOrderingIdQueryHandler(IRepository<OrderDetail> repository)
        {
            _repository = repository;
        }

        public async Task<List<GetOrderDetailQueryResult>> Handle(GetOrderDetailByOrderingIdQuery request, CancellationToken cancellationToken)
            => (await _repository.GetListByFilterAsync(x => x.OrderingId == request.OrderingId)).Select(x => x.ToResult()).ToList();
    }
}
