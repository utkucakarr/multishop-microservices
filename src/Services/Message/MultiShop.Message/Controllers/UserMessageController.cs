using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.Message.Dtos;
using MultiShop.Message.Services;

namespace MultiShop.Message.Controllers
{
    // Kullanıcılar arası özel yazışma: herkes (admin dahil) yalnızca kendi gelen/giden kutusunu görür.
    // Kullanıcı kimliği her zaman token'dan okunur; adres ya da gövdeden gelen kullanıcı ID'si kullanılmaz.
    [Authorize(Policy = MultiShopPolicies.Authenticated)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserMessageController : ControllerBase
    {
        private readonly IUserMessageService _userMessageService;
        private readonly ICurrentUserService _currentUser;

        public UserMessageController(IUserMessageService userMessageService, ICurrentUserService currentUser)
        {
            _userMessageService = userMessageService;
            _currentUser = currentUser;
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetMyInbox()
        {
            var values = await _userMessageService.GetInboxMessageAsync(_currentUser.GetRequiredUserId());
            return Ok(values);
        }

        [HttpGet("sendbox")]
        public async Task<IActionResult> GetMySendBox()
        {
            var values = await _userMessageService.GetSendBoxMessageAsync(_currentUser.GetRequiredUserId());
            return Ok(values);
        }

        [HttpGet("inbox/count")]
        public async Task<IActionResult> GetMyInboxCount()
        {
            var count = await _userMessageService.GetInboxMessageCountAsync(_currentUser.GetRequiredUserId());
            return Ok(count);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessageAsync(CreateMessageDto createMessageDto)
        {
            await _userMessageService.CreateMessageAsync(createMessageDto, _currentUser.GetRequiredUserId());
            return Ok("Mesaj başarıyla eklendi");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessageAsync(int id)
        {
            await _userMessageService.DeleteMessageAsync(id, _currentUser.GetRequiredUserId());
            return Ok("Mesaj başarıyla silindi");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMessageAsync(UpdateMessageDto updateMessageDto)
        {
            await _userMessageService.UpdateMessageAsync(updateMessageDto, _currentUser.GetRequiredUserId());
            return Ok("Mesaj başarıyla güncellendi");
        }

        /// <summary>Sistemdeki toplam mesaj sayısı (admin dashboard); mesaj içeriği dönmez.</summary>
        [Authorize(Policy = MultiShopPolicies.Admin)]
        [HttpGet("GetTotalMessageCount")]
        public async Task<IActionResult> GetTotalMessageCount()
        {
            var values = await _userMessageService.GetTotalMessageCountAsync();
            return Ok(values);
        }
    }
}
