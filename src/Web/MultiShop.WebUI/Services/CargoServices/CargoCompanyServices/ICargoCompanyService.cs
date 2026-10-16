using MultiShop.DtoLayer.CargoDtos.CargoCompanyDtos;

namespace MultiShop.WebUI.Services.CargoServices.CargoCompanyServices
{
    /// <summary>Cargo servisinin kargo firması istemcisi. Değiştiren işlemler başarıda null, değilse Cargo'nun mesajını döner.</summary>
    public interface ICargoCompanyService
    {
        Task<List<ResultCargoCompanyDto>> GetAllCargoCompanyAsync();

        /// <summary>Firma yoksa null.</summary>
        Task<UpdateCargoCompanyDto?> GetByIdCargoCompanyAsync(int id);

        Task<string?> CreateCargoCompanyAsync(CreateCargoCompanyDto createCargoCompanyDto);

        Task<string?> UpdateCargoCompanyAsync(UpdateCargoCompanyDto updateCargoCompanyDto);

        /// <summary>Gönderisi olan firma silinemez (Cargo 409 ve mesajı döner).</summary>
        Task<string?> DeleteCargoCompanyAsync(int id);

        /// <summary>Firmayı varsayılan yapar (yeni gönderilere atanır).</summary>
        Task<string?> SetDefaultCargoCompanyAsync(int id);
    }
}
