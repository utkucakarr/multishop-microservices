using MultiShop.Catalog.Dtos.StatisticDtos;

namespace MultiShop.Catalog.Services.StatisticServices
{
    public interface IStatisticService
    {
        /// <summary>Dashboard rakamlarının hepsi tek seferde (sorgular paralel çalışır).</summary>
        Task<CatalogStatisticsDto> GetStatisticsAsync();
    }
}
