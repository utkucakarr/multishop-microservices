using MultiShop.Catalog.Entities;

namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>Ürünü markası (<see cref="Product.Brand"/>) doldurulmuş olarak döner; yoksa null.</summary>
        Task<Product?> GetByIdWithBrandAsync(string id);
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<IEnumerable<Product>> GetProductsWithCategoryByCategoryIdAsync(string categoryId);
        /// <summary>Id geçerli biçimdeyse ve bu id'de marka varsa true.</summary>
        Task<bool> BrandExistsAsync(string brandId);
        /// <summary>Bu markaya bağlı ürün sayısı (marka silinmeden önce kontrol için).</summary>
        Task<long> CountByBrandIdAsync(string brandId);
    }
}
