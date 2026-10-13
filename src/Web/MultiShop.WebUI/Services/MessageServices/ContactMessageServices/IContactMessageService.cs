using MultiShop.DtoLayer.MessageDtos.ContactMessageDtos;

namespace MultiShop.WebUI.Services.MessageServices.ContactMessageServices
{
    public interface IContactMessageService
    {
        /// <summary>İletişim formunu gönderir; Message servisi kabul ederse true.</summary>
        Task<bool> CreateAsync(CreateContactMessageDto createContactMessageDto);
        Task<List<ResultContactMessageDto>> GetAllAsync(int? take = null);
        /// <summary>Mesaj yoksa null.</summary>
        Task<ResultContactMessageDto?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int id);
        Task DeleteAsync(int id);
    }
}
