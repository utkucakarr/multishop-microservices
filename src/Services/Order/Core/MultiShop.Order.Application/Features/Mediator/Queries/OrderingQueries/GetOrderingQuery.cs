using MediatR;
using MultiShop.Order.Application.Features.Mediator.Results.OrderingResults;
using MultiShop.Order.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.Mediator.Queries.OrderingQueries
{
    public class GetOrderingQuery:IRequest<List<GetOrderingQueryResult>>
    {
        /// <summary>Verilirse yalnızca bu durumdaki siparişler (admin listesi filtresi).</summary>
        public OrderStatus? Status { get; set; }
    }
}
