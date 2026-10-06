using MultiShop.Order.Application.Features.CQRS.Queries.OrderDetailQueries;
using MultiShop.Order.Application.Features.CQRS.Results.OrderDetailResults;
using MultiShop.Order.Application.Features.Mediator.Results.OrderingResults;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.CQRS.Handlers.OrderDetailHandlers
{
    public class GetOrderDetailByOrderingIdQueryHandler
    {
        private readonly IOrderDetailRepository _orderDetailRepository;

        public GetOrderDetailByOrderingIdQueryHandler(IOrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<List<GetOrderDetailByOrderingIdQueryResult>> Handle(GetOrderDetailByOrderingIdQuery query)
        {
            var values = _orderDetailRepository.GetOrderDetailByOrderingId(query.Id);
            return values.Select(x => new GetOrderDetailByOrderingIdQueryResult
            {
                OrderDetailId = x.OrderDetailId,
                OrderingId = x.OrderingId,
                ProductAmount = x.ProductAmount,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                ProductPrice = x.ProductPrice,
                ProductTotalPrice = x.ProductTotalPrice
            }).ToList();
        }
    }
}
