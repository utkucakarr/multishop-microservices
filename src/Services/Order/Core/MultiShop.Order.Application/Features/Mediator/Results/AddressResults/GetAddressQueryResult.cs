namespace MultiShop.Order.Application.Features.Mediator.Results.AddressResults
{
    public class GetAddressQueryResult
    {
        public int AddressId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string SurName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        /// <summary>Adres satırı 1. Önceden "Detail" adıyla dönüyordu; WebUI "Detail1" beklediği için sipariş sayfasında boş görünüyordu.</summary>
        public string Detail1 { get; set; } = string.Empty;

        public string Detail2 { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;
    }
}
