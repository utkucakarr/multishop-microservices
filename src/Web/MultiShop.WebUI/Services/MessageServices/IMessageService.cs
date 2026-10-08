using MultiShop.DtoLayer.MessageDtos;

namespace MultiShop.WebUI.Services.MessageServices
{
    public interface IMessageService
    {
        Task<List<ResultInboxMessageDto>> GetInboxMessageAsync();

        Task<List<ResultSendboxMessageDto>> GetSendboxMessageAsync();

        Task<int> GetInboxMessageCountAsync();
    }
}
