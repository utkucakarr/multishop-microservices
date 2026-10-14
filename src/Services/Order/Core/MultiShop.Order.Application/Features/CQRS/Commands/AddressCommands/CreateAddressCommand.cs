using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.CQRS.Commands.AddressCommands
{
    public class CreateAddressCommand
    {
        public int AddressId { get; set; }

        // İstekte gönderilmez; controller token'daki kullanıcıyı yazar.
        public string? UserId { get; set; }

        public string Name { get; set; }

        public string SurName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Country { get; set; }

        public string District { get; set; }

        public string City { get; set; }

        public string Detail1 { get; set; }

        // İsteğe bağlı alanlar: boş gelirse "" yazılır (sütunlar NOT NULL). Nullable olmayan string, ASP.NET'te
        // otomatik zorunlu sayıldığı için boş bırakılan adres satırı 2 isteği 400 ile reddettiriyordu.
        public string? Detail2 { get; set; }

        public string? Description { get; set; }

        public string? ZipCode { get; set; }
    }
}
