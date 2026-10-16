using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Message.Dtos.ContactMessageDtos;
using MultiShop.Message.Security;
using MultiShop.Message.Services;

namespace MultiShop.Message.Controllers
{
    // İletişim formu mesajları: ziyaretçi (giriş yapmadan) gönderebilir, yalnızca Admin okur/siler.
    // Yetki uç bazında verilir: sınıfa konan bir [Authorize] POST'a da eklenirdi (kurallar birleşir, biri diğerini ezmez).
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactsController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [Authorize(Policy = MessagePolicies.ContactWrite)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateContactMessageDto createContactMessageDto)
        {
            var id = await _contactMessageService.CreateAsync(createContactMessageDto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        /// <param name="take">Verilirse yalnızca en yeni bu kadar mesaj döner (ör. admin üst menüsü için 5).</param>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? take)
        {
            var values = await _contactMessageService.GetAllAsync(take);
            return Ok(values);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var value = await _contactMessageService.GetByIdAsync(id);
            return Ok(value);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _contactMessageService.MarkAsReadAsync(id);
            return NoContent();
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactMessageService.DeleteAsync(id);
            return NoContent();
        }
    }
}
