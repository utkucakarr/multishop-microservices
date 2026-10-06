using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Order.Application.Features.CQRS.Commands.OrderDetailCommands;
using MultiShop.Order.Application.Features.CQRS.Handlers.OrderDetailHandlers;
using MultiShop.Order.Application.Features.CQRS.Queries.OrderDetailQueries;

namespace MultiShop.Order.WebApi.Controllers
{
    [Authorize]
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

        public OrderDetailsController(GetOrderDetailQueryHandler getOrderDetailQueryHandler, GetOrderDetailByIdQueryHandler getOrderDetailByIdQueryHandler, CreateOrderDetailCommandHandler createOrderDetailCommandHandler, UpdateOrderDetailQueryHandler updateOrderDetailQueryHandler, RemoveOrderDetailQueryHandler removeOrderDetailQueryHandler, GetOrderDetailByOrderingIdQueryHandler getOrderDetailByOrderingIdQueryHandler)
        {
            _getOrderDetailQueryHandler = getOrderDetailQueryHandler;
            _getOrderDetailByIdQueryHandler = getOrderDetailByIdQueryHandler;
            _createOrderDetailCommandHandler = createOrderDetailCommandHandler;
            _updateOrderDetailQueryHandler = updateOrderDetailQueryHandler;
            _removeOrderDetailQueryHandler = removeOrderDetailQueryHandler;
            _getOrderDetailByOrderingIdQueryHandler = getOrderDetailByOrderingIdQueryHandler;
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetailList()
        {
            var values = await _getOrderDetailQueryHandler.Handle();
            return Ok(values);
        }

        [HttpGet("GetOrderDetailById")]
        public async Task<IActionResult> GetOrderDetailById(int id)
        {
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
            await _createOrderDetailCommandHandler.Handle(command);
            return Ok("Sipariş detayı başarıyla eklendi");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrderDetail(UpdateOrderDetailCommand command)
        {
            await _updateOrderDetailQueryHandler.Handle(command);
            return Ok("Sipariş detayı başarıyla güncellendi");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveOrderDetail(int id)
        {
            await _removeOrderDetailQueryHandler.Handle(new RemoveOrderDetailCommand(id));
            return Ok("Sipariş detayı başarıyla silindi");
        }
    }
}
