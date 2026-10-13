using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.CatalogDtos.ProductDtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }

        public decimal ProductPrice { get; set; }

        public string ProductImageUrl { get; set; }

        public string ProductDescription { get; set; }

        public string CategoryId { get; set; }
        // İsteğe bağlı marka; "Marka yok" seçilirse null
        public string? BrandId { get; set; }
        // Ürün sisteme ilk girildiğindeki başlangıç stoğu
        public int AvailableStock { get; set; }
        // Stok bu seviyenin altına inince yeniden sipariş uyarısı
        public int RestockThreshold { get; set; } = 5;
        // Deponun alabileceği maksimum stok (minimum stoktan büyük olmalı)
        public int MaxStockThreshold { get; set; } = 100;
    }
}
