using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.MessageDtos.ContactMessageDtos;
using MultiShop.WebUI.Services.MessageServices.ContactMessageServices;

namespace MultiShop.WebUI.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        public IActionResult Index()
        {
            SetBreadcrumb();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CreateContactMessageDto createContactMessageDto)
        {
            if (ModelState.IsValid)
            {
                if (await _contactMessageService.CreateAsync(createContactMessageDto))
                {
                    // Post/Redirect/Get: sayfa yenilenince form tekrar gönderilmez.
                    TempData["ContactSent"] = true;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Mesajınız şu an gönderilemedi, lütfen biraz sonra tekrar deneyin.");
            }

            SetBreadcrumb();
            return View(createContactMessageDto);
        }

        private void SetBreadcrumb()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "İletişim";
            ViewBag.directory3 = "Mesaj Gönder";
        }
    }
}
