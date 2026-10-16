using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace MultiShop.WebUI.Services
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Başarılıysa null; değilse servisin ProblemDetails yanıtındaki <c>detail</c> mesajı
        /// (ör. Catalog: "Bu markaya bağlı 3 ürün var ..."). <c>detail</c> yoksa doğrulama hatalarının mesajları
        /// (<c>errors</c>; [ApiController]'ın otomatik 400 yanıtı). Mesaj okunamazsa genel bir metin döner.
        /// </summary>
        public static async Task<string?> GetErrorMessageAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return null;

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                    return problem.Detail;
                if (problem?.Errors.Count > 0)
                    return string.Join(" ", problem.Errors.SelectMany(x => x.Value));
            }
            catch (Exception)
            {
                // Gövde ProblemDetails değil (ör. gateway'in boş 401/403 yanıtı).
            }

            return $"İşlem başarısız oldu ({(int)response.StatusCode}).";
        }

        /// <summary>
        /// Başarılıysa gövdeyi okur; değilse <see cref="ServiceApiException"/> fırlatır (sayfa okuyan GET'ler için).
        /// Hata, kullanıcıya genel bir sayfa olarak gösterilir; ayrıntısı Seq'e yazılır.
        /// </summary>
        public static async Task<T> ReadOrThrowAsync<T>(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ServiceApiException(response.StatusCode, response.RequestMessage?.RequestUri, await response.GetErrorMessageAsync());

            return (await response.Content.ReadFromJsonAsync<T>())!;
        }
    }
}
