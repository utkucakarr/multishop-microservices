namespace MultiShop.DtoLayer.StatisticDtos
{
    /// <summary>
    /// Ocelot <c>GET /services/admin/dashboard</c> yanıtı. Yanıt vermeyen servisin alanı null gelir;
    /// ekranda o servisin kutuları "—" gösterir.
    /// </summary>
    public class AdminDashboardDto
    {
        public CatalogStatisticsDto? Catalog { get; set; }
        public CommentStatisticsDto? Comment { get; set; }
        public DiscountStatisticsDto? Discount { get; set; }
        public MessageStatisticsDto? Message { get; set; }
        public IdentityStatisticsDto? Identity { get; set; }
    }

    public class CatalogStatisticsDto
    {
        public long ProductCount { get; set; }
        public long CategoryCount { get; set; }
        public long BrandCount { get; set; }
        public decimal AveragePrice { get; set; }
        public string MostExpensiveProduct { get; set; } = string.Empty;
        public string CheapestProduct { get; set; } = string.Empty;
    }

    public class CommentStatisticsDto
    {
        public int Total { get; set; }
        /// <summary>Admin'in onayladığı yorumlar.</summary>
        public int Approved { get; set; }
        /// <summary>Onay bekleyen yorumlar.</summary>
        public int Pending { get; set; }
    }

    public class DiscountStatisticsDto
    {
        public int CouponCount { get; set; }
        /// <summary>Aktif ve süresi dolmamış kuponlar.</summary>
        public int ActiveCouponCount { get; set; }
    }

    public class MessageStatisticsDto
    {
        /// <summary>Toplam iletişim formu mesajı.</summary>
        public int ContactMessages { get; set; }
        public int Unread { get; set; }
    }

    public class IdentityStatisticsDto
    {
        public int UserCount { get; set; }
    }
}
