using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MultiShop.BuildingBlocks.Exceptions;
using MultiShop.Contracts.Messages;
using MultiShop.Message.DAL.Context;
using MultiShop.Message.DAL.Entities;
using MultiShop.Message.Dtos;
using MultiShop.Message.Dtos.ContactMessageDtos;
// MassTransit'te de MessageContext adında bir tür var; buradaki bizim DbContext'imiz.
using MessageContext = MultiShop.Message.DAL.Context.MessageContext;

namespace MultiShop.Message.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly MessageContext _messageContext;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public ContactMessageService(MessageContext messageContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _messageContext = messageContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task CreateAsync(CreateContactMessageDto createContactMessageDto)
        {
            var value = _mapper.Map<ContactMessage>(createContactMessageDto);
            value.IsRead = false;
            value.SendDate = DateTime.UtcNow;

            // Mesaj ve ContactMessageReceived tek transaction'da yazılır (transactional outbox): ya ikisi birden kaydedilir ya hiçbiri.
            // Publish RabbitMQ'ya değil OutboxMessage tablosuna yazar; event'te mesaj id'si olduğu için önce mesaj kaydedilir.
            await using var transaction = await _messageContext.Database.BeginTransactionAsync();

            await _messageContext.ContactMessages.AddAsync(value);
            await _messageContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new ContactMessageReceived(
                value.ContactMessageId, value.NameSurname, value.Email, value.Subject, value.SendDate));
            await _messageContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task<List<ResultContactMessageDto>> GetAllAsync(int? take = null)
        {
            IQueryable<ContactMessage> query = _messageContext.ContactMessages
                .AsNoTracking()
                .OrderByDescending(x => x.SendDate);
            if (take is > 0)
                query = query.Take(take.Value);
            var values = await query.ToListAsync();
            return _mapper.Map<List<ResultContactMessageDto>>(values);
        }

        public async Task<ResultContactMessageDto> GetByIdAsync(int id)
            => _mapper.Map<ResultContactMessageDto>(await FindAsync(id));

        public async Task MarkAsReadAsync(int id)
        {
            var value = await FindAsync(id);
            value.IsRead = true;
            await _messageContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var value = await FindAsync(id);
            _messageContext.ContactMessages.Remove(value);
            await _messageContext.SaveChangesAsync();
        }

        public async Task<MessageStatisticsDto> GetStatisticsAsync() => new()
        {
            ContactMessages = await _messageContext.ContactMessages.CountAsync(),
            Unread = await _messageContext.ContactMessages.CountAsync(x => !x.IsRead)
        };

        private async Task<ContactMessage> FindAsync(int id)
            => await _messageContext.ContactMessages.FindAsync(id)
               ?? throw new NotFoundException("İletişim mesajı", id);
    }
}
