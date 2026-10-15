using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Order.Application.Features.Mediator.Queries.OrderDetailQueries;
using MultiShop.Order.WebApi.Security;

namespace MultiShop.Order.WebApi.Controllers
{
    // Sipariş satırları yalnızca okunur: siparişle birlikte saga yazar (ORD-02). Admin'in satır düzenleme/silme uçları
    // kaldırıldı; satır tutarlarını sipariş toplamından (ve ödenen tutardan) koparabiliyordu.
    // Satırlara erişim, bağlı oldukları siparişin sahipliğine göre kontrol edilir.
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly OrderAccessGuard _accessGuard;

        public OrderDetailsController(IMediator mediator, OrderAccessGuard accessGuard)
        {
            _mediator = mediator;
            _accessGuard = accessGuard;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> OrderDetailList()
            => Ok(await _mediator.Send(new GetOrderDetailQuery()));

        /// <summary>Bir siparişin satırları (önceden adı yanıltıcı <c>GetOrderDetailById?id=</c> idi, ORD-03).</summary>
        [HttpGet("by-ordering/{orderingId:int}")]
        public async Task<IActionResult> OrderDetailsByOrderingId(int orderingId)
        {
            await _accessGuard.EnsureOrderingAccessAsync(orderingId);
            return Ok(await _mediator.Send(new GetOrderDetailByOrderingIdQuery(orderingId)));
        }
    }
}
