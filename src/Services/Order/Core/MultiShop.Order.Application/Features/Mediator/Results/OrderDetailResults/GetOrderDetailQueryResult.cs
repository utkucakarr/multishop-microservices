namespace MultiShop.Order.Application.Features.Mediator.Results.OrderDetailResults
{
    public class GetOrderDetailQueryResult
    {
        public int OrderDetailId { get; set; }

        public int OrderingId { get; set; }

        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal ProductPrice { get; set; }

        public int ProductAmount { get; set; }

        public decimal ProductTotalPrice { get; set; }
    }
}
