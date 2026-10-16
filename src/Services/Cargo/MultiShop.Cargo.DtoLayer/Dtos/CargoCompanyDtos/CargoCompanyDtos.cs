using System.ComponentModel.DataAnnotations;
using MultiShop.Cargo.EntityLayer.Concrete;

namespace MultiShop.Cargo.DtoLayer.Dtos.CargoCompanyDtos
{
    /// <summary>Yeni firma ya da ad değişikliği (güncellemede id route'tan gelir; varsayılanlık ayrı uçla değişir).</summary>
    public class SaveCargoCompanyDto
    {
        [Required(ErrorMessage = "Firma adı boş olamaz.")]
        [MaxLength(CargoCompany.NameMaxLength, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
        public string CargoCompanyName { get; set; } = string.Empty;
    }

    public record CargoCompanyDto(int CargoCompanyId, string CargoCompanyName, bool IsDefault);
}
