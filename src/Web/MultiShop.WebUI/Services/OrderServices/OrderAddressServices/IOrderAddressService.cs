using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderAddressServices
{
    public interface IOrderAddressService
    {
        /// <summary>Adresi kaydeder; oluşan adresin id'si ya da Order'ın hata mesajı.</summary>
        Task<(int? AddressId, string? Error)> CreateOrderAddressAsync(CreateOrderAddressDto createOrderAddressDto);

        /// <summary>Kullanıcının kendi kayıtlı adresleri (Admin için de yalnızca kendisininki).</summary>
        Task<List<ResultOrderAddressDto>> GetMyAddressesAsync();
    }
}
