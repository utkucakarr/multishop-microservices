using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Message.Dtos;
using MultiShop.Message.Services;

namespace MultiShop.Message.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMessageController : ControllerBase
    {
        private readonly IUserMessageService _userMessageService;

        public UserMessageController(IUserMessageService userMessageService)
        {
            _userMessageService = userMessageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessage()
        {
            var values = await _userMessageService.GetAllMessageAsync();
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessageAsync(CreateMessageDto createMessageDto)
        {
            await _userMessageService.CreateMessageAsync(createMessageDto);
            return Ok("Mesaj başarıyla eklendi");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessageAsync(int id)
        {
            await _userMessageService.DeleteMessageAsync(id);
            return Ok("Mesaj başarıyla silindi");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMessageAsync(UpdateMessageDto updateMessageDto)
        {
            await _userMessageService.UpdateMessageAsync(updateMessageDto);
            return Ok("Mesaj başarıyla güncellendi");
        }

        [HttpGet("GetMessageSendBox")]
        public async Task<IActionResult> GetMessageSendBox(string id)
        {
            var values = await _userMessageService.GetSendBoxMessageAsync(id);
            return Ok(values);
        }

        [HttpGet("GetMesssageInBox")]
        public async Task<IActionResult> GetMessageInBox(string id)
        {
            var values = await _userMessageService.GetInboxMessageAsync(id);
            return Ok(values);
        }

        [HttpGet("GetTotalMessageCount")]
        public async Task<IActionResult> GetTotalMessageCount()
        {
            var values = await _userMessageService.GetTotalMessageCountAsync();
            return Ok(values);
        }

        [HttpGet("GetTotalMessageCountByRecieverId")]
        public async Task<IActionResult> GetTotalMessageCountByRecieverId(string id)
        {
            int values = await _userMessageService.GetTotalMessageCountByRecieverId(id);
            return Ok(values);
        }
    }
}
