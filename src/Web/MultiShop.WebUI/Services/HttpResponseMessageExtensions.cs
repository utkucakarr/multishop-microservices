using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace MultiShop.WebUI.Services
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Başarılıysa null; değilse servisin ProblemDetails yanıtındaki <c>detail</c> mesajı
        /// (ör. Catalog: "Bu markaya bağlı 3 ürün var ..."). Mesaj okunamazsa genel bir metin döner.
        /// </summary>
        public static async Task<string?> GetErrorMessageAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return null;

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                    return problem.Detail;
            }
            catch (Exception)
            {
                // Gövde ProblemDetails değil (ör. gateway'in boş 401/403 yanıtı).
            }

            return $"İşlem başarısız oldu ({(int)response.StatusCode}).";
        }
    }
}
