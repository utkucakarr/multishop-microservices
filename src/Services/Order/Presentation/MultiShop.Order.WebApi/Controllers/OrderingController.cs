using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Order.Application.Features.CQRS.Commands.OrderDetailCommands;
using MultiShop.Order.Application.Features.Mediator.Commands.OrderingCommands;
using MultiShop.Order.Application.Features.Mediator.Queries.OrderingQueries;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Order.WebApi.Security;

namespace MultiShop.Order.WebApi.Controllers
{
    // Müşteri yalnızca kendi siparişlerine, Admin hepsine erişir; kullanıcı kimliği her zaman token'dan okunur.
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        private readonly OrderAccessGuard _accessGuard;

        public OrderingController(IMediator mediator, ICurrentUserService currentUser, OrderAccessGuard accessGuard)
        {
            _mediator = mediator;
            _currentUser = currentUser;
            _accessGuard = accessGuard;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> OrderingList()
        {
            var values = await _mediator.Send(new GetOrderingQuery());
            return Ok(values);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> OrderingById(int id)
        {
            await _accessGuard.EnsureOrderingAccessAsync(id);
            var value = await _mediator.Send(new GetOrderingByIdQuery(id));
            return Ok(value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrdering(CreateOrderingCommand command)
        {
            // İstekteki UserId yok sayılır: kimse başkası adına sipariş açamaz.
            command.UserId = _currentUser.GetRequiredUserId();
            var orderingId = await _mediator.Send(command);
            return Ok(orderingId);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> RemoveOrdering(int id)
        {
            await _mediator.Send(new RemoveOrderingCommand(id));
            return Ok("Sipariş başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateOrdering(UpdateOrderingCommand command)
        {
            await _mediator.Send(command);
            return Ok("Sipariş başarıyla güncellendi");
        }

        /// <summary>Giriş yapmış kullanıcının kendi siparişleri ("Siparişlerim").</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyOrderings()
        {
            var values = await _mediator.Send(new GetOrderingByUserIdQuery(_currentUser.GetRequiredUserId()));
            return Ok(values);
        }

        /// <summary>Belirli bir kullanıcının siparişleri; yalnızca Admin (müşteri için <c>mine</c>).</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetOrderingByUserId")]
        public async Task<IActionResult> GetOrderingByUserId(string id)
        {
            var values = await _mediator.Send(new GetOrderingByUserIdQuery(id));
            return Ok(values);
        }
    }
}
