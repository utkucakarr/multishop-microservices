using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Cargo.EntityLayer.Concrete;
using MultiShop.Cargo.BusinessLayer.Services;

namespace MultiShop.Cargo.WebApi.Controllers
{
    // Gönderiler sipariş ödenince otomatik açılır (Order saga'sı → CreateShipment); admin yalnızca durumunu ilerletir.
    [Authorize(Policy = MultiShopPolicies.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentsController : ControllerBase
    {
        private readonly ShipmentService _shipmentService;

        public ShipmentsController(ShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>Gönderiler (en yeni önce); <paramref name="status"/> verilirse yalnızca o durumdakiler.</summary>
        [HttpGet]
        public async Task<IActionResult> GetShipments([FromQuery] ShipmentStatus? status)
            => Ok(await _shipmentService.GetAllAsync(status));

        /// <summary>Gönderi, alıcı adresi ve hareket geçmişi.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetShipment(int id)
            => Ok(await _shipmentService.GetAsync(id));

        /// <summary>Kargoya ver: sipariş "Kargoda" olur, müşteriye takip numarasıyla mail gider. Hazırlanmıyorsa ya da firma yoksa 409.</summary>
        [HttpPost("{id:int}/dispatch")]
        public async Task<IActionResult> Dispatch(int id)
        {
            await _shipmentService.DispatchAsync(id);
            return NoContent();
        }

        /// <summary>Teslim edildi: sipariş tamamlanır. Gönderi kargoda değilse 409.</summary>
        [HttpPost("{id:int}/deliver")]
        public async Task<IActionResult> Deliver(int id)
        {
            await _shipmentService.DeliverAsync(id);
            return NoContent();
        }
    }
}
