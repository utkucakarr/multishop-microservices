using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    // Sitedeki iletişim formundan gelen mesajlar: listeleme, okuma (açınca okundu olur), silme.
    [Authorize(Roles = MultiShopRoles.Admin)]
    [Area("Admin")]
    [Route("Admin/ContactMessage")]
    public class ContactMessageController : Controller
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactMessageController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        void ContactMessageViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Mesajlar";
            ViewBag.v3 = "İletişim Mesajları";
            ViewBag.v0 = "Mesaj İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ContactMessageViewBagList();
            var values = await _contactMessageService.GetAllAsync();
            return View(values);
        }

        [Route("Detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            ContactMessageViewBagList();
            var value = await _contactMessageService.GetByIdAsync(id);
            if (value is null)
                return NotFound();

            if (!value.IsRead)
            {
                await _contactMessageService.MarkAsReadAsync(id);
                value.IsRead = true;
            }
            return View(value);
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactMessageService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
