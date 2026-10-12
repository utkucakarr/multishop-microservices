namespace MultiShop.Catalog.Dtos.StatisticDtos
{
    /// <summary>Admin dashboard'unun Catalog kısmı (<c>GET api/statistics</c>).</summary>
    public class CatalogStatisticsDto
    {
        public long ProductCount { get; set; }
        public long CategoryCount { get; set; }
        public long BrandCount { get; set; }
        public decimal AveragePrice { get; set; }
        public string MostExpensiveProduct { get; set; } = string.Empty;
        public string CheapestProduct { get; set; } = string.Empty;
    }
}
