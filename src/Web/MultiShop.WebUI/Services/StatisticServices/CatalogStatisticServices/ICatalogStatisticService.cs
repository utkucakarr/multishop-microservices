namespace MultiShop.WebUI.Services.StatisticServices.CatalogStatisticServices
{
    public interface ICatalogStatisticService
    {
        Task<long> GetCategoryCountAsync();

        Task<long> GetProductCountAsync();

        Task<long> GetBrandCountAsync();

        Task<decimal> GetProductAvgPriceAsync();

        Task<string> GetMaxPriceProductNameAsync();

        Task<string> GetMinPriceProductNameAsync();
    }
}
