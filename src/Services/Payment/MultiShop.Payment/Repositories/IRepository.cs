using System.Linq.Expressions;

namespace MultiShop.Payment.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id);

        Task<T> CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        //Burada bir giriş ve bir çıkış değeri istiyor. Örnek olarak giriş değerinde ankara var ise bool değer true dönücek değil ise false dönücek.
        Task<T> GetByFilterAsync(Expression<Func<T, bool>> filter);
    }
}
