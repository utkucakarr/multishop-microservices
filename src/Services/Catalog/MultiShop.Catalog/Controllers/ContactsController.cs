using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Catalog.Dtos.ContactDtos;
using MultiShop.Catalog.Services.ContactServices;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Catalog.Security;

namespace MultiShop.Catalog.Controllers
{
    // İletişim mesajları: ziyaretçi gönderebilir, yalnızca Admin okur/siler.
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet]
        public async Task<IActionResult> ContactList()
        {
            var contacts = await _contactService.GetAllContactAsync();
            return Ok(contacts);
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(string id)
        {
            var contactId = await _contactService.GetByIdContactAsync(id);
            return Ok(contactId);
        }

        [Authorize(Policy = CatalogPolicies.ContactWrite)]
        [HttpPost]
        public async Task<IActionResult> CreateContact(CreateContactDto createContactDto)
        {
            await _contactService.CreateContactAsync(createContactDto);
            return Ok("İletişim bilgileri başarıyla eklendi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpDelete]
        public async Task<IActionResult> DeleteContact(string id)
        {
            await _contactService.DeleteContactAsync(id);
            return Ok("İletişim bilgileri başarıyla silindi");
        }

        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpPut]
        public async Task<IActionResult> UpdateContact(UpdateContactDto updateContactDto)
        {
            await _contactService.UpdateContactAsync(updateContactDto);
            return Ok("İletişim biligleri başarıyla güncellendi");
        }
    }
}