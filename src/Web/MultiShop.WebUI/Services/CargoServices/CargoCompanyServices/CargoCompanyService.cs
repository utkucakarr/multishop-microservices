using System.Net;
using MultiShop.DtoLayer.CargoDtos.CargoCompanyDtos;

namespace MultiShop.WebUI.Services.CargoServices.CargoCompanyServices
{
    public class CargoCompanyService : ICargoCompanyService
    {
        private readonly HttpClient _httpClient;

        public CargoCompanyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultCargoCompanyDto>> GetAllCargoCompanyAsync()
            => await _httpClient.GetFromJsonAsync<List<ResultCargoCompanyDto>>("cargocompanies") ?? new();

        public async Task<UpdateCargoCompanyDto?> GetByIdCargoCompanyAsync(int id)
        {
            var response = await _httpClient.GetAsync($"cargocompanies/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UpdateCargoCompanyDto>();
        }

        public async Task<string?> CreateCargoCompanyAsync(CreateCargoCompanyDto createCargoCompanyDto)
            => await (await _httpClient.PostAsJsonAsync("cargocompanies", createCargoCompanyDto)).GetErrorMessageAsync();

        public async Task<string?> UpdateCargoCompanyAsync(UpdateCargoCompanyDto updateCargoCompanyDto)
            => await (await _httpClient.PutAsJsonAsync($"cargocompanies/{updateCargoCompanyDto.CargoCompanyId}",
                new { updateCargoCompanyDto.CargoCompanyName })).GetErrorMessageAsync();

        public async Task<string?> DeleteCargoCompanyAsync(int id)
            => await (await _httpClient.DeleteAsync($"cargocompanies/{id}")).GetErrorMessageAsync();

        public async Task<string?> SetDefaultCargoCompanyAsync(int id)
            => await (await _httpClient.PostAsync($"cargocompanies/{id}/default", null)).GetErrorMessageAsync();
    }
}
