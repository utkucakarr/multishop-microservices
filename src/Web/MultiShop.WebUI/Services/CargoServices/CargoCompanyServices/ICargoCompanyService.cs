using MultiShop.DtoLayer.CargoDtos.CargoCompanyDtos;

namespace MultiShop.WebUI.Services.CargoServices.CargoCompanyServices
{
    public interface ICargoCompanyService
    {
        Task<List<ResultCargoCompanyDto>> GetAllCargoCompanyAsync();

        Task CreateCargoCompanyAsync(CreateCargoCompanyDto createCargoCompanyDto);

        Task UpdateCargoCompanyAsync(UpdateCargoCompanyDto updateCargoCompanyDto);

        Task DeleteCargoCompanyAsync(int id);

        Task<UpdateCargoCompanyDto> GetByIdCargoCompanyAsync(int id);

        /// <summary>Firmayı varsayılan yapar (yeni gönderilere atanır). Başarıda null, değilse Cargo'nun mesajı.</summary>
        Task<string?> SetDefaultCargoCompanyAsync(int id);
    }
}
