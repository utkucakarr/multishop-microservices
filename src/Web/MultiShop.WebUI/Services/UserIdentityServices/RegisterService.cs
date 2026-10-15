using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;
using Newtonsoft.Json;
using System.Net;

namespace MultiShop.WebUI.Services.UserIdentityServices
{
    // Yeni üye kaydı doğrudan IdentityServer'a gider (Ocelot'ta Identity route'u yok; login de böyle).
    // Uç [AllowAnonymous] olduğu için token eklenmez.
    public class RegisterService : IRegisterService
    {
        private readonly HttpClient _httpClient;

        public RegisterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string[]?> RegisterAsync(CreateRegisterDto createRegisterDto)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("api/registers", createRegisterDto);
            if (responseMessage.IsSuccessStatusCode)
                return null;

            // IdentityServer, Identity'nin hata nedenlerini (şifre kuralları, kullanılan kullanıcı adı) 400 + { errors: [...] } ile döner.
            if (responseMessage.StatusCode == HttpStatusCode.BadRequest
                && ReadIdentityErrors(await responseMessage.Content.ReadAsStringAsync()) is { Length: > 0 } errors)
                return errors;

            // Diğer durumlar (ör. model doğrulama hatası, sunucu hatası): ortak ProblemDetails okuyucusu.
            return new[] { await responseMessage.GetErrorMessageAsync() ?? "Kayıt sırasında bir hata oluştu, tekrar deneyin." };
        }

        private static string[]? ReadIdentityErrors(string body)
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(body, new { errors = Array.Empty<string>() })?.errors;
            }
            catch (JsonException)
            {
                // Farklı biçimdeki yanıt (ör. ValidationProblemDetails'te errors bir sözlüktür).
                return null;
            }
        }
    }
}
