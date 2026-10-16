namespace MultiShop.Cargo.EntityLayer.Concrete
{
    public class CargoCompany
    {
        public const int NameMaxLength = 100;

        public int CargoCompanyId { get; set; }

        public string CargoCompanyName { get; set; } = string.Empty;

        /// <summary>Yeni gönderilere otomatik atanan firma; aynı anda yalnızca biri. Hiçbiri seçili değilse ilk firma.</summary>
        public bool IsDefault { get; set; }
    }
}
