using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using MultiShop.Payment.Settings;

namespace MultiShop.Payment.Services.GarantiPosServices
{
    /// <summary>Bankaya giden kart bilgisi. Yalnızca bu istekte kullanılır; hiçbir yere yazılmaz, loglanmaz.</summary>
    public record CardDetails(string Number, string Month, string Year, string Cvv);

    /// <summary>Bankanın cevabı: onay ya da kullanıcıya gösterilebilir red mesajı.</summary>
    public record GarantiPosResult(bool IsSuccess, string? ErrorMessage);

    /// <summary>Banka cevap vermedi ya da cevabı okunamadı: çekimin olup olmadığı bilinmiyor.</summary>
    public class GarantiPosException : Exception
    {
        public GarantiPosException(string message, Exception? innerException = null) : base(message, innerException)
        { }
    }

    /// <summary>
    /// Garanti sanal POS satış isteği (typed HttpClient, IHttpClientFactory; PAY-02).
    /// Retry bilerek yok: satış isteği idempotent değil, yeniden göndermek iki kez çekim demek olabilir.
    /// </summary>
    public class GarantiPosClient
    {
        private static readonly Encoding TurkishEncoding = CreateTurkishEncoding();

        private readonly HttpClient _httpClient;
        private readonly GarantiPosSettings _settings;
        private readonly ILogger<GarantiPosClient> _logger;

        public GarantiPosClient(HttpClient httpClient, IOptions<GarantiPosSettings> settings, ILogger<GarantiPosClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <param name="orderId">Bankadaki sipariş kimliği; her deneme için farklı olmalı.</param>
        /// <param name="amount">TL; Garanti kuruş ister (123.45 TL → 12345).</param>
        public async Task<GarantiPosResult> SaleAsync(string orderId, decimal amount, CardDetails card)
        {
            var amountInKurus = (ulong)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
            var expireDate = int.Parse(card.Month, CultureInfo.InvariantCulture).ToString("00") + card.Year[^2..];
            var hashData = GetHashData(_settings.ProvisionPassword, _settings.TerminalId, orderId, card.Number, amountInKurus, _settings.CurrencyCode);

            var request = new XElement("GVPSRequest",
                new XElement("Mode", _settings.Mode),
                new XElement("Version", "512"),
                new XElement("Terminal",
                    new XElement("ProvUserID", _settings.ProvUserId),
                    new XElement("HashData", hashData),
                    new XElement("UserID", _settings.ProvUserId),
                    new XElement("ID", _settings.TerminalId),
                    new XElement("MerchantID", _settings.MerchantId)),
                new XElement("Customer",
                    new XElement("IPAddress", _settings.CustomerIpAddress),
                    new XElement("EmailAddress", _settings.CustomerEmailAddress)),
                new XElement("Card",
                    new XElement("Number", card.Number),
                    new XElement("ExpireDate", expireDate),
                    new XElement("CVV2", card.Cvv)),
                new XElement("Order",
                    new XElement("OrderID", orderId),
                    new XElement("GroupID")),
                new XElement("Transaction",
                    new XElement("Type", "sales"),
                    new XElement("Amount", amountInKurus),
                    new XElement("CurrencyCode", _settings.CurrencyCode),
                    new XElement("CardholderPresentCode", "0"),
                    new XElement("MotoInd", "N")));

            var xml = "<?xml version='1.0' encoding='iso-8859-9'?>\n" + request;
            using var content = new StringContent(xml, TurkishEncoding, "application/xml");

            string responseBody;
            try
            {
                using var response = await _httpClient.PostAsync(_settings.ApiUrl, content);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Banka isteği başarısız (sipariş {BankOrderId}).", orderId);
                throw new GarantiPosException("Banka şu an yanıt vermiyor, lütfen biraz sonra tekrar deneyin.", ex);
            }

            XDocument document;
            try
            {
                document = XDocument.Parse(responseBody);
            }
            catch (XmlException ex)
            {
                // Ham cevap loglanmaz (G-10); uzunluğu teşhis için yeterli.
                _logger.LogWarning(ex, "Banka cevabı okunamadı (sipariş {BankOrderId}, {Length} karakter).", orderId, responseBody.Length);
                throw new GarantiPosException("Bankanın cevabı okunamadı, lütfen biraz sonra tekrar deneyin.", ex);
            }

            var code = document.Descendants("Code").FirstOrDefault()?.Value;
            var reasonCode = document.Descendants("ReasonCode").FirstOrDefault()?.Value;
            var message = document.Descendants("Message").FirstOrDefault()?.Value;
            var errorMessage = document.Descendants("ErrorMsg").FirstOrDefault()?.Value;

            var isSuccess = code == "00" && reasonCode == "00" && message == "Approved";
            if (isSuccess)
                return new GarantiPosResult(true, null);

            _logger.LogInformation("Banka ödemeyi onaylamadı (sipariş {BankOrderId}, kod {Code}/{ReasonCode}).", orderId, code, reasonCode);
            return new GarantiPosResult(false, string.IsNullOrWhiteSpace(errorMessage) ? "Ödeme banka tarafından onaylanmadı." : errorMessage);
        }

        // Garanti'nin hash'i: SHA512(orderId + terminalId + kart + tutar + para birimi + SHA1(şifre + "0" + terminalId)), büyük harf hex.
        private static string GetHashData(string provisionPassword, string terminalId, string orderId, string cardNumber, ulong amount, int currencyCode)
        {
            var hashedPassword = Hex(SHA1.HashData(TurkishEncoding.GetBytes(provisionPassword + "0" + terminalId)));
            return Hex(SHA512.HashData(TurkishEncoding.GetBytes(orderId + terminalId + cardNumber + amount + currencyCode + hashedPassword)));
        }

        private static string Hex(byte[] bytes) => Convert.ToHexString(bytes);

        private static Encoding CreateTurkishEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("ISO-8859-9");
        }
    }
}
