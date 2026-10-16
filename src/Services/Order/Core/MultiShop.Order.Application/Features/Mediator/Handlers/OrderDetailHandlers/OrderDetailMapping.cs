using MultiShop.Order.Application.Features.Mediator.Results.OrderDetailResults;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.OrderDetailHandlers
{
    internal static class OrderDetailMapping
    {
        public static GetOrderDetailQueryResult ToResult(this OrderDetail detail) => new()
        {
            OrderDetailId = detail.OrderDetailId,
            OrderingId = detail.OrderingId,
            ProductId = detail.ProductId,
            ProductName = detail.ProductName,
            ProductPrice = detail.ProductPrice,
            ProductAmount = detail.ProductAmount,
            ProductTotalPrice = detail.ProductTotalPrice
        };
    }
}
