using System.Linq.Expressions;
using MultiShop.Order.Application.Interfaces;

namespace MultiShop.Order.UnitTests.Fakes
{
    /// <summary>Saga activity'lerinin kullandığı <see cref="IRepository{T}"/> için bellekte çalışan sahte depo.</summary>
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _items = new();
        private readonly Func<T, int> _getId;
        private readonly Action<T, int> _setId;
        private int _lastId;

        public InMemoryRepository(Func<T, int> getId, Action<T, int> setId)
        {
            _getId = getId;
            _setId = setId;
        }

        public IReadOnlyList<T> Items
        {
            get { lock (_items) return _items.ToList(); }
        }

        public Task<List<T>> GetAllAsync() => Task.FromResult(Items.ToList());

        public Task<T> GetByIdAsync(int id) => Task.FromResult(Items.SingleOrDefault(x => _getId(x) == id)!);

        public Task<T> CreateAsync(T entity)
        {
            lock (_items)
            {
                if (_getId(entity) == 0)
                    _setId(entity, ++_lastId);
                _items.Add(entity);
            }
            return Task.FromResult(entity);
        }

        // Nesneler referansla tutulduğu için güncelleme ayrıca bir şey yapmaz.
        public Task UpdateAsync(T entity) => Task.CompletedTask;

        public Task DeleteAsync(T entity)
        {
            lock (_items) _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<T> GetByFilterAsync(Expression<Func<T, bool>> filter)
            => Task.FromResult(Items.AsQueryable().FirstOrDefault(filter)!);

        public Task<List<T>> GetListByFilterAsync(Expression<Func<T, bool>> filter)
            => Task.FromResult(Items.AsQueryable().Where(filter).ToList());
    }
}
