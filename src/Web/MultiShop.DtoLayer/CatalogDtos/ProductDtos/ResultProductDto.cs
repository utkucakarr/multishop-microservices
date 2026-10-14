using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.CatalogDtos.ProductDtos
{
    public class ResultProductDto
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal ProductPrice { get; set; }

        public string ProductImageUrl { get; set; }

        public string ProductDescription { get; set; }

        public string CategoryId { get; set; }
        public string? BrandId { get; set; }

        /// <summary>Eldeki stok.</summary>
        public int AvailableStock { get; set; }

        /// <summary>Ödemesi beklenen siparişlere ayrılmış; satılabilir = eldeki − ayrılmış.</summary>
        public int ReservedStock { get; set; }
    }
}
