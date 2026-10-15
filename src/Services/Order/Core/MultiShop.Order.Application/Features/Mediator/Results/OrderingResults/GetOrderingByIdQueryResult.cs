using MultiShop.Order.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.Mediator.Results.OrderingResults
{
    public class GetOrderingByIdQueryResult
    {
        public int OrderingId { get; set; }

        public Guid OrderNumber { get; set; }

        public string UserId { get; set; }

        public int AddressId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }

        /// <summary>İptal sebebi ya da son başarısız ödeme denemesinin sebebi.</summary>
        public string? FailMessage { get; set; }
    }
}
