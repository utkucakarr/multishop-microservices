namespace MultiShop.DtoLayer.OrderDtos.OrderAddressDtos
{
    /// <summary>Kullanıcının kayıtlı adresi (sipariş sayfasında seçmek için).</summary>
    public class ResultOrderAddressDto
    {
        public int AddressId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SurName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Detail1 { get; set; } = string.Empty;

        public string? Detail2 { get; set; }

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string? ZipCode { get; set; }
    }
}
