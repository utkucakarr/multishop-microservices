using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Message.DAL.Context;
using MultiShop.Message.DAL.Entities;
using MultiShop.Message.Dtos;

namespace MultiShop.Message.Services
{
    public class UserMessageService : IUserMessageService
    {
        private readonly MessageContext _messageContext;
        private readonly IMapper _mapper;

        public UserMessageService(MessageContext messageContext, IMapper mapper)
        {
            _messageContext = messageContext;
            _mapper = mapper;
        }

        public async Task CreateMessageAsync(CreateMessageDto createMessageDto, string senderId)
        {
            var value = _mapper.Map<UserMessage>(createMessageDto);
            // İstekteki SenderId yok sayılır: kimse başkası adına mesaj gönderemez.
            value.SenderId = senderId;
            await _messageContext.UserMessages.AddAsync(value);
            await _messageContext.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(int id, string userId)
        {
            var value = await FindOwnMessageAsync(id, userId);
            _messageContext.UserMessages.Remove(value);
            await _messageContext.SaveChangesAsync();
        }

        public async Task<List<ResultInboxMessageDto>> GetInboxMessageAsync(string userId)
        {
            var values = await _messageContext.UserMessages.Where(x => x.ReveiverId == userId).ToListAsync();
            return _mapper.Map<List<ResultInboxMessageDto>>(values);
        }

        public async Task<List<ResultSendBoxMessageDto>> GetSendBoxMessageAsync(string userId)
        {
            var values = await _messageContext.UserMessages.Where(x => x.SenderId == userId).ToListAsync();
            return _mapper.Map<List<ResultSendBoxMessageDto>>(values);
        }

        public async Task<int> GetTotalMessageCountAsync()
            => await _messageContext.UserMessages.CountAsync();

        public async Task<int> GetInboxMessageCountAsync(string userId)
            => await _messageContext.UserMessages.CountAsync(x => x.ReveiverId == userId);

        public async Task UpdateMessageAsync(UpdateMessageDto updateMessageDto, string userId)
        {
            var value = await FindOwnMessageAsync(updateMessageDto.UserMessageId, userId);
            // Gönderen, alıcı ve tarih değiştirilemez; yalnızca içerik ve okundu bilgisi güncellenir.
            value.Subject = updateMessageDto.Subject;
            value.MessageDetail = updateMessageDto.MessageDetail;
            value.IsRead = updateMessageDto.IsRead;
            await _messageContext.SaveChangesAsync();
        }

        // Mesaj yoksa ya da kullanıcı göndericisi/alıcısı değilse 404: başkasının mesaj numarasını deneyen,
        // o numarada bir mesaj olduğunu da öğrenemez.
        private async Task<UserMessage> FindOwnMessageAsync(int id, string userId)
            => await _messageContext.UserMessages
                   .FirstOrDefaultAsync(x => x.UserMessageId == id && (x.SenderId == userId || x.ReveiverId == userId))
               ?? throw new NotFoundException("Mesaj", id);
    }
}
