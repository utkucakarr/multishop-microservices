using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;

namespace MultiShop.WebUI.Services.UserIdentityServices
{
    public interface IRegisterService
    {
        /// <summary>Kayıt başarılıysa null; değilse kullanıcıya gösterilecek hata mesajları.</summary>
        Task<string[]?> RegisterAsync(CreateRegisterDto createRegisterDto);
    }
}
