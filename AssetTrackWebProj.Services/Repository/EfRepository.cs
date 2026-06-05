using System.Linq.Expressions;
using AssetTrack.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetTrack.Services.Repository
{
    /// <summary>
    /// EF Core implementation of <see cref="IRepository{T}"/>.
    /// </summary>
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _set;

        public EfRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<T>();
        }

        public IQueryable<T> All() => _set;

        public IQueryable<T> AllAsNoTracking() => _set.AsNoTracking();

        public async Task<T?> GetByIdAsync(params object[] id) => await _set.FindAsync(id);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _set.FirstOrDefaultAsync(predicate);

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public void Update(T entity) => _set.Update(entity);

        public void Delete(T entity) => _set.Remove(entity);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
