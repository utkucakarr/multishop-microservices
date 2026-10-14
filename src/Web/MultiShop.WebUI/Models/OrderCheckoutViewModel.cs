using System.Globalization;
using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;

namespace MultiShop.WebUI.Models
{
    /// <summary>Sipariş onay sayfası: kayıtlı adreslerden biri ya da yeni adres, ve kullanıcının gördüğü genel toplam.</summary>
    public class OrderCheckoutViewModel
    {
        public List<ResultOrderAddressDto> Addresses { get; set; } = new();

        /// <summary>Seçilen kayıtlı adres; 0 = yeni adres formu.</summary>
        public int SelectedAddressId { get; set; }

        public CreateOrderAddressDto NewAddress { get; set; } = new();

        /// <summary>
        /// Sayfada gösterilen genel toplam (gizli alan, "123.45"); Basket onayda bununla karşılaştırır. Kültürden bağımsız
        /// metin: istek kültürü (tr/en/fr) değişse de "42.00" 4200 olarak okunmaz.
        /// </summary>
        public string ExpectedTotal { get; set; } = "0";

        public static string FormatTotal(decimal total) => total.ToString("0.00", CultureInfo.InvariantCulture);

        public decimal ExpectedTotalAmount
            => decimal.TryParse(ExpectedTotal, NumberStyles.Number, CultureInfo.InvariantCulture, out var total) ? total : 0;
    }
}
