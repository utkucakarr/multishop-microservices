using System.ComponentModel.DataAnnotations;

namespace MultiShop.Catalog.Dtos.ProductDtos
{
    /// <summary>Admin'in girdiği eldeki stok. Ödemesi beklenen siparişlere ayrılmış adetten az olamaz.</summary>
    public class UpdateProductStockDto
    {
        [Range(0, 1_000_000)]
        public int AvailableStock { get; set; }
    }
}
