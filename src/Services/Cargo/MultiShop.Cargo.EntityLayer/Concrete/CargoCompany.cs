namespace MultiShop.Cargo.EntityLayer.Concrete
{
    public class CargoCompany
    {
        public int CargoCompanyId { get; set; }

        public string CargoCompanyName { get; set; }

        /// <summary>Yeni gönderilere otomatik atanan firma; aynı anda yalnızca biri. Hiçbiri seçili değilse ilk firma.</summary>
        public bool IsDefault { get; set; }
    }
}
