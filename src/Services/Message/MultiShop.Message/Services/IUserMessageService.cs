using MultiShop.Message.Dtos;

namespace MultiShop.Message.Services
{
    // Tüm metotlar işlemi yapan kullanıcının kimliğini (token'daki "sub") alır;
    // kullanıcı yalnızca göndericisi ya da alıcısı olduğu mesajlara erişebilir.
    public interface IUserMessageService
    {
        Task<List<ResultInboxMessageDto>> GetInboxMessageAsync(string userId);

        Task<List<ResultSendBoxMessageDto>> GetSendBoxMessageAsync(string userId);

        Task CreateMessageAsync(CreateMessageDto createMessageDto, string senderId);

        Task UpdateMessageAsync(UpdateMessageDto updateMessageDto, string userId);

        Task DeleteMessageAsync(int id, string userId);

        Task<int> GetTotalMessageCountAsync();

        Task<int> GetInboxMessageCountAsync(string userId);
    }
}
