using System.ComponentModel.DataAnnotations;
using MultiShop.Message.DAL.Entities;

namespace MultiShop.Message.Dtos.ContactMessageDtos
{
    /// <summary>Ziyaretçinin gönderdiği alanlar. Tarih ve okundu bilgisi istemciden alınmaz, sunucu atar.</summary>
    public class CreateContactMessageDto
    {
        [Required, StringLength(ContactMessageLimits.NameSurname)]
        public string NameSurname { get; set; }

        [Required, EmailAddress, StringLength(ContactMessageLimits.Email)]
        public string Email { get; set; }

        [Required, StringLength(ContactMessageLimits.Subject)]
        public string Subject { get; set; }

        [Required, StringLength(ContactMessageLimits.Message)]
        public string Message { get; set; }
    }
}
