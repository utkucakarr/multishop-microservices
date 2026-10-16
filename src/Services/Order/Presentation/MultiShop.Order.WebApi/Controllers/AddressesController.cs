using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands;
using MultiShop.Order.Application.Features.Mediator.Queries.AddressQueries;
using MultiShop.Order.WebApi.Security;

namespace MultiShop.Order.WebApi.Controllers
{
    // Müşteri yalnızca kendi adreslerini görür ve yönetir, Admin hepsini. Komutlar handler'dan önce doğrulanır (400).
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        private readonly OrderAccessGuard _accessGuard;

        public AddressesController(IMediator mediator, ICurrentUserService currentUser, OrderAccessGuard accessGuard)
        {
            _mediator = mediator;
            _currentUser = currentUser;
            _accessGuard = accessGuard;
        }

        /// <summary>
        /// Müşteri için kendi adresleri. Admin için tüm adresler; <paramref name="userId"/> verilirse yalnızca o kullanıcınınkiler
        /// (admin panelinde kullanıcının adres bilgisi).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddressList([FromQuery] string? userId)
        {
            // Önceden herkes tüm kullanıcıların adreslerini (ad, telefon, açık adres) listeleyebiliyordu.
            var filter = _currentUser.IsAdmin ? userId : _currentUser.GetRequiredUserId();
            return Ok(await _mediator.Send(new GetAddressQuery(filter)));
        }

        /// <summary>
        /// Giriş yapmış kullanıcının kendi adresleri; Admin için de yalnızca kendisininki. Sipariş verirken seçilen adres
        /// bunlardan biri olmalı (Basket sepet onayında buna bakar), Admin başkasının adresine sipariş açamaz.
        /// </summary>
        [HttpGet("mine")]
        public async Task<IActionResult> MyAddresses()
            => Ok(await _mediator.Send(new GetAddressQuery(_currentUser.GetRequiredUserId())));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> AddressById(int id)
        {
            await _accessGuard.EnsureAddressAccessAsync(id);
            return Ok(await _mediator.Send(new GetAddressByIdQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress(CreateAddressCommand command)
        {
            command.UserId = _currentUser.GetRequiredUserId();
            // Id, sepet onayında teslimat adresi olarak Basket'e gönderilir.
            var addressId = await _mediator.Send(command);
            return CreatedAtAction(nameof(AddressById), new { id = addressId }, new { addressId });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressCommand command)
        {
            // Adres başka bir kullanıcıya devredilemez: sahibi değişmeden kalır.
            var address = await _accessGuard.EnsureAddressAccessAsync(id);
            command.AddressId = id;
            command.UserId = address.UserId;
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveAddress(int id)
        {
            await _accessGuard.EnsureAddressAccessAsync(id);
            await _mediator.Send(new RemoveAddressCommand(id));
            return NoContent();
        }
    }
}
