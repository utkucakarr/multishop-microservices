using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Interfaces
{
    //buradaki T dışarıdan bir T değeri alıcak demek ve bu T değeri mutlaka bir class olmalı
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id);

        Task<T> CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        //Burada bir giriş ve bir çıkış değeri istiyor. Örnek olarak giriş değerinde ankara var ise bool değer true dönücek değil ise false dönücek.
        Task<T> GetByFilterAsync(Expression<Func<T, bool>> filter);

        Task<List<T>> GetListByFilterAsync(Expression<Func<T, bool>> filter);
    }
}
