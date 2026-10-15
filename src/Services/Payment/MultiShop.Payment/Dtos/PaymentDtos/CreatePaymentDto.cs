using System.ComponentModel.DataAnnotations;

namespace MultiShop.Payment.Dtos.PaymentDtos
{
    /// <summary>
    /// Kullanıcının ödeme isteği: hangi sipariş ve kart. Tutar ve kullanıcı istekte yok; tutar Order'ın bildirdiği
    /// ödeme kaydından, kullanıcı token'dan alınır. Kart bilgisi yalnızca bankaya gider, saklanmaz.
    /// </summary>
    public class CreatePaymentDto
    {
        public Guid OrderNumber { get; set; }

        [Required(ErrorMessage = "Kart sahibinin adı zorunlu.")]
        public string CardOwnerName { get; set; } = null!;

        [Required(ErrorMessage = "Kart numarası zorunlu.")]
        [RegularExpression(@"^\d{12,19}$", ErrorMessage = "Kart numarası 12-19 haneli olmalı, yalnızca rakam.")]
        public string CardNumber { get; set; } = null!;

        [Required(ErrorMessage = "Son kullanma ayı zorunlu.")]
        [RegularExpression(@"^(0?[1-9]|1[0-2])$", ErrorMessage = "Son kullanma ayı 1-12 olmalı.")]
        public string Month { get; set; } = null!;

        [Required(ErrorMessage = "Son kullanma yılı zorunlu.")]
        [RegularExpression(@"^(\d{2}|\d{4})$", ErrorMessage = "Son kullanma yılı 2 ya da 4 haneli olmalı.")]
        public string Year { get; set; } = null!;

        [Required(ErrorMessage = "CVV zorunlu.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV 3-4 haneli olmalı.")]
        public string Cvv { get; set; } = null!;
    }
}
