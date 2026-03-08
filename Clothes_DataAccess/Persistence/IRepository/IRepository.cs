using System.Linq.Expressions;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default);

        Task<T> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool isTracking = false,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task CreateAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
