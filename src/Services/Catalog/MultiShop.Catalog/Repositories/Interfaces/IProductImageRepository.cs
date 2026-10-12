using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IProductImageRepository : IGenericRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(string productId);
        Task<IDictionary<string, string>> GetMainImageUrlsAsync(IEnumerable<string> productIds);
        /// <summary>Ürünün en büyük DisplayOrder değeri; görseli yoksa 0.</summary>
        Task<int> GetMaxDisplayOrderAsync(string productId);
        /// <summary>Ürünün tüm görsellerini ana görsel olmaktan çıkarır (yeni ana görsel seçilmeden önce).</summary>
        Task ClearMainAsync(string productId);
        Task DeleteByProductIdAsync(string productId);
    }
}
