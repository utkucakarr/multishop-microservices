using MultiShop.Payment.Dtos.PaymentDtos;
using System.Text;
using System.Security.Cryptography;
using MultiShop.Payment.Repositories;
using MultiShop.Payment.Entities;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Extensions.Options;
using MultiShop.Payment.Settings;

namespace MultiShop.Payment.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentInfo> _repository;
        private readonly GarantiPosSettings _posSettings;

        public PaymentService(IRepository<PaymentInfo> repository, IOptions<GarantiPosSettings> posSettings)
        {
            _repository = repository;
            _posSettings = posSettings.Value;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var orderId = "MLT-SHP-" + createPaymentDto.OrderingId.ToString(); //Guid.NewGuid().ToString("N");
            // Tutar kültürden bağımsız ("123.45") gelir; Garanti kuruş cinsinden ister (123.45 TL -> 12345).
            if (!decimal.TryParse(createPaymentDto.PaymentAmounth, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
            {
                return new CreatePaymentResponseDto { IsSuccess = false, ErrorMessage = "Geçersiz ödeme tutarı." };
            }
            var paymentAmounth = (ulong)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
            //Güvenlik için hash oluşturuluyor. POS bilgileri GarantiPosSettings'ten (user-secrets) okunur.
            var hashData = GetHashData(_posSettings.ProvisionPassword, _posSettings.TerminalId, orderId, createPaymentDto.CardNumber, paymentAmounth, _posSettings.CurrencyCode);
            var xmlData = $"<?xml version='1.0' encoding='iso-8859-9'?>\n" +
                      $"<GVPSRequest>\n" +
                      $"    <Mode>{_posSettings.Mode}</Mode>\n" +
                      $"    <Version>512</Version>\n" +
                      $"    <Terminal>\n" +
                      $"        <ProvUserID>{_posSettings.ProvUserId}</ProvUserID>\n" +
                      $"        <HashData>{hashData}</HashData>\n" +
                      $"        <UserID>{_posSettings.ProvUserId}</UserID>\n" +
                      $"        <ID>{_posSettings.TerminalId}</ID>\n" +
                      $"        <MerchantID>{_posSettings.MerchantId}</MerchantID>\n" +
                      $"    </Terminal>\n" +
                      $"    <Customer>\n" +
                      $"        <IPAddress>{_posSettings.CustomerIpAddress}</IPAddress>\n" +
                      $"        <EmailAddress>{_posSettings.CustomerEmailAddress}</EmailAddress>\n" +
                      $"    </Customer>\n" +
                      $"    <Card>\n" +
                      $"        <Number>{createPaymentDto.CardNumber}</Number>\n" +
                      $"        <ExpireDate>{createPaymentDto.Month+createPaymentDto.Year.Substring(createPaymentDto.Year.Length-2)}</ExpireDate>\n" +
                      $"        <CVV2>{createPaymentDto.Cvv}</CVV2>\n" +
                      $"    </Card>\n" +
                      $"    <Order>\n" +
                      $"        <OrderID>{orderId}</OrderID>\n" +
                      $"        <GroupID />\n" +
                      $"    </Order>\n" +
                      $"    <Transaction>\n" +
                      $"        <Type>sales</Type>\n" +
                      $"        <Amount>{paymentAmounth}</Amount>\n" +
                      $"        <CurrencyCode>{_posSettings.CurrencyCode}</CurrencyCode>\n" +
                      $"        <CardholderPresentCode>0</CardholderPresentCode>\n" +
                      $"        <MotoInd>N</MotoInd>\n" +
                      $"    </Transaction>\n" +
                      $"</GVPSRequest>";

            var client = new HttpClient();

            var requestContent = new StringContent(xmlData, Encoding.GetEncoding("iso-8859-9"), "application/xml");

            var response = await client.PostAsync(_posSettings.ApiUrl, requestContent);

            string responseString = await response.Content.ReadAsStringAsync();

            var xml = XDocument.Parse(responseString); // Gelen xml'e dönüştürüyor.
            var code = xml.Descendants("Code").FirstOrDefault()?.Value;
            var reasonCode = xml.Descendants("ReasonCode").FirstOrDefault()?.Value;
            var message = xml.Descendants("Message").FirstOrDefault()?.Value;
            var errorMessage = xml.Descendants("ErrorMsg").FirstOrDefault()?.Value;

            var isSuccess = code == "00" && reasonCode == "00" && message == "Approved";

            await _repository.CreateAsync(new PaymentInfo
            {
                UserId = createPaymentDto.UserId,
                OrderingId = createPaymentDto.OrderingId,
                BankResponse = responseString,
                IsSuccess = isSuccess,
                CreatedDate = DateTime.Now,
                CardNumber = createPaymentDto.CardNumber,
                Amount = amount
            });

            var paymentResponse = new CreatePaymentResponseDto
            {
                ErrorMessage = errorMessage,
                IsSuccess = isSuccess
            };

            return paymentResponse;
        }

        //Eski sistemlerde kullanıyor daha az güvenli açık var
        public static string Sha1(string text)
        {
            var provider = CodePagesEncodingProvider.Instance; // Iso "ISO-8859-9" kodunu getiriyor
            Encoding.RegisterProvider(provider); //Türkçe karaktere çevirmek için iso kodunu

            var cryptoServiceProvider = new SHA1CryptoServiceProvider(); //cryptoservice'e sha1 şifreleme işlemini uyguluyoruz.
            var inputbytes = cryptoServiceProvider.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text)); // hash ile şifreliyoruz.

            var builder = new StringBuilder();
            for (int i = 0; i < inputbytes.Length; i++)
            {
                builder.Append(string.Format("{0,2:x}", inputbytes[i]).Replace(" ", "0")); //Bu satırda her bir byte hexadecimal (onaltılık) forma çevrilir, tek basamaklı olanlara başına 0 konur.
            }

            return builder.ToString().ToUpper(); //Sonuç olarak elde edilen değer büyük harflerle döndürülür:
        }

        //Yeni sistemlerde kullanılıyor
        public static string Sha512(string text)
        {
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            var cryptoServiceProvider = new SHA512CryptoServiceProvider();
            var inputbytes = cryptoServiceProvider.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text));

            var builder = new StringBuilder();
            for (int i = 0; i < inputbytes.Length; i++)
            {
                builder.Append(string.Format("{0,2:x}", inputbytes[i]).Replace(" ", "0"));
            }

            return builder.ToString().ToUpper();
        }

        public static string GetHashData(string userPassword, string terminalId, string orderId, string cardNumber, ulong amount, int currencyCode)
        {
            var hashedPassword = Sha1(userPassword + "0" + terminalId);
            return Sha512(orderId + terminalId + cardNumber + amount + currencyCode + hashedPassword).ToUpper();
        }
    }
}
