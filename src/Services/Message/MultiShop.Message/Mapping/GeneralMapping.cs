using AutoMapper;
using MultiShop.Message.DAL.Entities;
using MultiShop.Message.Dtos.ContactMessageDtos;

namespace MultiShop.Message.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping()
        {
            CreateMap<CreateContactMessageDto, ContactMessage>();
            CreateMap<ContactMessage, ResultContactMessageDto>();
        }
    }
}
