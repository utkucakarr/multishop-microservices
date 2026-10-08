using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.MessageServices;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Authorize]
    [Area("User")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<IActionResult> Inbox()
        {
            var values = await _messageService.GetInboxMessageAsync();
            return View(values);
        }

        public async Task<IActionResult> Sendbox()
        {
            var values = await _messageService.GetSendboxMessageAsync();
            return View(values);
        }
    }
}
