using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Order.Application.Features.CQRS.Commands.OrderDetailCommands;
using MultiShop.Order.Application.Features.CQRS.Handlers.OrderDetailHandlers;
using MultiShop.Order.Application.Features.CQRS.Queries.OrderDetailQueries;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Order.WebApi.Security;

namespace MultiShop.Order.WebApi.Controllers
{
    // Sipariş detayına erişim, bağlı olduğu siparişin sahipliğine göre kontrol edilir.
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly GetOrderDetailQueryHandler _getOrderDetailQueryHandler;
        private readonly GetOrderDetailByIdQueryHandler _getOrderDetailByIdQueryHandler;
        private readonly CreateOrderDetailCommandHandler _createOrderDetailCommandHandler;
        private readonly UpdateOrderDetailQueryHandler _updateOrderDetailQueryHandler;
        private readonly RemoveOrderDetailQueryHandler _removeOrderDetailQueryHandler;
        private readonly GetOrderDetailByOrderingIdQueryHandler _getOrderDetailByOrderingIdQueryHandler;
        private readonly OrderAccessGuard _accessGuard;

        public OrderDetailsController(OrderAccessGuard accessGuard, GetOrderDetailQueryHandler getOrderDetailQueryHandler, GetOrderDetailByIdQueryHandler getOrderDetailByIdQueryHandler, CreateOrderDetailCommandHandler createOrderDetailCommandHandler, UpdateOrderDetailQueryHandler updateOrderDetailQueryHandler, RemoveOrderDetailQueryHandler removeOrderDetailQueryHandler, GetOrderDetailByOrderingIdQueryHandler getOrderDetailByOrderingIdQueryHandler)
        {
            _getOrderDetailQueryHandler = getOrderDetailQueryHandler;
            _getOrderDetailByIdQueryHandler = getOrderDetailByIdQueryHandler;
            _createOrderDetailCommandHandler = createOrderDetailCommandHandler;
            _updateOrderDetailQueryHandler = updateOrderDetailQueryHandler;
            _removeOrderDetailQueryHandler = removeOrderDetailQueryHandler;
            _getOrderDetailByOrderingIdQueryHandler = getOrderDetailByOrderingIdQueryHandler;
            _accessGuard = accessGuard;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> OrderDetailList()
        {
            var values = await _getOrderDetailQueryHandler.Handle();
            return Ok(values);
        }

        // Not: adına rağmen parametre sipariş numarasıdır (OrderingId); o siparişin satırlarını döner.
        [HttpGet("GetOrderDetailById")]
        public async Task<IActionResult> GetOrderDetailById(int id)
        {
            await _accessGuard.EnsureOrderingAccessAsync(id);
            var value = await _getOrderDetailByIdQueryHandler.Handle(new GetOrderDetailByQuery(id));
            return Ok(value);
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetOrderDetailByOrderingId(int id)
        //{
        //    var value = await _getOrderDetailByOrderingIdQueryHandler.Handle(new GetOrderDetailByOrderingIdQuery(id));
        //    return Ok(value);
        //}

        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail(CreateOrderDetailCommand command)
        {
            // Satır yalnızca kullanıcının kendi siparişine eklenebilir.
            await _accessGuard.EnsureOrderingAccessAsync(command.OrderingId);
            await _createOrderDetailCommandHandler.Handle(command);
            return Ok("Sipariş detayı başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateOrderDetail(UpdateOrderDetailCommand command)
        {
            await _updateOrderDetailQueryHandler.Handle(command);
            return Ok("Sipariş detayı başarıyla güncellendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> RemoveOrderDetail(int id)
        {
            await _removeOrderDetailQueryHandler.Handle(new RemoveOrderDetailCommand(id));
            return Ok("Sipariş detayı başarıyla silindi");
        }
    }
}
