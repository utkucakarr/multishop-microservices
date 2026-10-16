using System.Text.Json.Serialization;

namespace MultiShop.Order.Application.Features.Mediator.Commands.AddressCommands
{
    /// <summary>Adres oluşturma ve güncellemenin ortak alanları; doğrulama kuralları da ortak (<c>AddressCommandValidator</c>).</summary>
    public abstract class AddressCommandBase
    {
        /// <summary>İstekten okunmaz: oluştururken token'daki kullanıcı, güncellerken adresin sahibi yazılır.</summary>
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string SurName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Detail1 { get; set; } = string.Empty;

        // İsteğe bağlı alanlar: boş gelirse "" yazılır (sütunlar NOT NULL). Nullable olmayan string, ASP.NET'te
        // otomatik zorunlu sayıldığı için boş bırakılan adres satırı 2 isteği 400 ile reddettiriyordu.
        public string? Detail2 { get; set; }

        public string? Description { get; set; }

        public string? ZipCode { get; set; }
    }
}
