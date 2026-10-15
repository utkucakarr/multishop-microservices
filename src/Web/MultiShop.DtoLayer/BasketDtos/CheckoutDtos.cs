namespace MultiShop.DtoLayer.BasketDtos
{
    /// <summary>Sepet onayı: teslimat adresi ve kullanıcının onay ekranında gördüğü genel toplam.</summary>
    public class CheckoutDto
    {
        public int AddressId { get; set; }

        /// <summary>Basket'in o anki toplamı bundan farklıysa (fiyat arada değişti) sipariş açılmaz.</summary>
        public decimal ExpectedTotal { get; set; }
    }

    public class CheckoutResultDto
    {
        public Guid OrderNumber { get; set; }
    }
}
