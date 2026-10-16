using MultiShop.Message.Dtos;
using MultiShop.Message.Dtos.ContactMessageDtos;

namespace MultiShop.Message.Services
{
    public interface IContactMessageService
    {
        /// <returns>Oluşan mesajın id'si.</returns>
        Task<int> CreateAsync(CreateContactMessageDto createContactMessageDto);
        Task<List<ResultContactMessageDto>> GetAllAsync(int? take = null);
        Task<ResultContactMessageDto> GetByIdAsync(int id);
        Task MarkAsReadAsync(int id);
        Task DeleteAsync(int id);
        Task<MessageStatisticsDto> GetStatisticsAsync();
    }
}
