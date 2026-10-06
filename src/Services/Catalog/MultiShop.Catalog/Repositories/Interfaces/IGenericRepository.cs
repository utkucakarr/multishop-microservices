namespace MultiShop.Catalog.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task CreateAsync(T entity);
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
    }
}
