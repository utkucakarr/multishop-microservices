namespace MultiShop.Catalog.Services.StatisticServices
{
    public interface IStatisticService
    {
        Task<long> GetCategoryCountAsync();

        Task<long> GetProductCountAsync();

        Task<long> GetBrandCountAsync();

        Task<decimal> GetProductAvgPriceAsync();

        Task<string> GetMaxPriceProductNameAsync();

        Task<string> GetMinPriceProductNameAsync();
    }
}
