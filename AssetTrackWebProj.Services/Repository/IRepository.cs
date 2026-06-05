using System.Linq.Expressions;

namespace AssetTrack.Services.Repository
{
    /// <summary>
    /// Thin generic repository abstraction over EF Core. Exists so the service layer
    /// can be unit tested with Moq without touching a real database.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IQueryable<T> All();

        IQueryable<T> AllAsNoTracking();

        Task<T?> GetByIdAsync(params object[] id);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<int> SaveChangesAsync();
    }
}
