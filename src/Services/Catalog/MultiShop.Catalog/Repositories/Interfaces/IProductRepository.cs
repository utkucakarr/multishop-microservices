using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>Ürünü markası (<see cref="Product.Brand"/>) doldurulmuş olarak döner; yoksa null.</summary>
        Task<Product?> GetByIdWithBrandAsync(string id);
        /// <summary>Verilen id'lerdeki ürünler tek sorguda; geçersiz biçimdeki ya da olmayan id'ler sonuçta yer almaz.</summary>
        Task<List<Product>> GetByIdsAsync(IEnumerable<string> ids);
        /// <summary>
        /// Admin'in değiştirdiği alanları ($set) yazar; stok alanlarına dokunmaz. Dokümanın tamamını yeniden yazmak (Replace),
        /// form açıkken yapılan bir stok rezervasyonunu eski değerle ezerdi.
        /// </summary>
        Task UpdateDetailsAsync(Product product);
        /// <summary>Eldeki stoğu ayarlar; ayrılmış adetten azsa yazmaz ve false döner (ürün yoksa da false).</summary>
        Task<bool> SetAvailableStockAsync(string productId, int availableStock);
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<IEnumerable<Product>> GetProductsWithCategoryByCategoryIdAsync(string categoryId);
        /// <summary>Id geçerli biçimdeyse ve bu id'de marka varsa true.</summary>
        Task<bool> BrandExistsAsync(string brandId);
        /// <summary>Bu markaya bağlı ürün sayısı (marka silinmeden önce kontrol için).</summary>
        Task<long> CountByBrandIdAsync(string brandId);
        /// <summary>Bu kategorideki ürün sayısı (kategori silinmeden önce kontrol için).</summary>
        Task<long> CountByCategoryIdAsync(string categoryId);
    }
}
