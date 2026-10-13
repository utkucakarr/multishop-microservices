using System.ComponentModel.DataAnnotations;

namespace MultiShop.DtoLayer.MessageDtos.ContactMessageDtos
{
    /// <summary>İletişim formu. Tarih ve okundu bilgisini Message servisi atar.</summary>
    public class CreateContactMessageDto
    {
        [Required(ErrorMessage = "Lütfen adınızı ve soyadınızı yazın.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string NameSurname { get; set; }

        [Required(ErrorMessage = "Lütfen e-posta adresinizi yazın.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yazın.")]
        [StringLength(254)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lütfen bir konu yazın.")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Lütfen mesajınızı yazın.")]
        [StringLength(4000, ErrorMessage = "Mesaj en fazla 4000 karakter olabilir.")]
        public string Message { get; set; }
    }
}
