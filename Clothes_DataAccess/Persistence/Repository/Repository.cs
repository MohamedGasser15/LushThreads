using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    #region Repository Class
    /// <summary>
    /// Generic repository implementation for CRUD operations with Entity Framework Core
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        #region Fields
        private readonly ApplicationDbContext _db;
        private readonly ILogger<Repository<T>> _logger;
        internal DbSet<T> dbSet;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the Repository class
        /// </summary>
        /// <param name="db">Application database context</param>
        /// <param name="logger">Logger instance for logging operations</param>
        /// <exception cref="ArgumentNullException">Thrown when db or logger is null</exception>
        public Repository(ApplicationDbContext db, ILogger<Repository<T>> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbSet = _db.Set<T>();
        }

        public Repository(ApplicationDbContext db)
        {
            _db = db;
        }
        #endregion

        #region Read Operations
        /// <summary>
        /// Retrieves all entities with optional filtering, ordering, and including related properties
        /// </summary>
        /// <param name="filter">Filter expression to apply on entities</param>
        /// <param name="includeProperties">Comma-separated related properties to include in the query</param>
        /// <param name="isTracking">Whether to enable entity tracking (default: false)</param>
        /// <param name="orderBy">Ordering function to sort the results</param>
        /// <param name="take">Number of records to take (limit results)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>List of entities matching the criteria</returns>
        public async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? take = null,
            CancellationToken cancellationToken = default)
        {
            const string operationName = "GetAllAsync";

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                IQueryable<T> query = isTracking ? dbSet : dbSet.AsNoTracking();

                if (filter != null)
                {
                    query = query.Where(filter);
                    _logger.LogDebug("Filter applied in {OperationName}", operationName);
                }

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProperty.Trim());
                    }
                    _logger.LogDebug("Include properties applied: {IncludeProperties}", includeProperties);
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                    _logger.LogDebug("Ordering applied in {OperationName}", operationName);
                }

                if (take.HasValue && take.Value > 0)
                {
                    query = query.Take(take.Value);
                    _logger.LogDebug("Take limit applied: {TakeValue}", take.Value);
                }

                var result = await query.ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully retrieved {Count} entities of type {EntityType} in {OperationName}",
                    result.Count, typeof(T).Name, operationName);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single entity based on filter criteria
        /// </summary>
        /// <param name="filter">Filter expression to find the entity</param>
        /// <param name="includeProperties">Comma-separated related properties to include</param>
        /// <param name="isTracking">Whether to enable entity tracking (default: false)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Entity matching the filter or null if not found</returns>
        /// <exception cref="ArgumentNullException">Thrown when filter is null</exception>
        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool isTracking = false,
            CancellationToken cancellationToken = default)
        {
            const string operationName = "GetAsync";

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                IQueryable<T> query = isTracking ? dbSet : dbSet.AsNoTracking();

                query = query.Where(filter);

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProperty.Trim());
                    }
                    _logger.LogDebug("Include properties applied: {IncludeProperties}", includeProperties);
                }

                var result = await query.FirstOrDefaultAsync(cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("No entity found matching the filter in {OperationName} for type {EntityType}",
                        operationName, typeof(T).Name);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved entity of type {EntityType} in {OperationName}",
                        typeof(T).Name, operationName);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Checks if any entity satisfies the given condition
        /// </summary>
        /// <param name="predicate">Condition to check against entities</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>True if any entity exists matching the predicate, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            const string operationName = "AnyAsync";

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                var result = await dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);

                _logger.LogInformation("Completed {OperationName} for entity type {EntityType} with result: {Result}",
                    operationName, typeof(T).Name, result);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }
        #endregion

        #region Create Operations
        /// <summary>
        /// Creates a new entity in the database
        /// </summary>
        /// <param name="entity">Entity to create</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            const string operationName = "CreateAsync";

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                await dbSet.AddAsync(entity, cancellationToken);
                await SaveAsync(cancellationToken);

                _logger.LogInformation("Successfully created entity of type {EntityType} in {OperationName}",
                    typeof(T).Name, operationName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            const string operationName = "UpdateAsync";

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                dbSet.Update(entity);
                await SaveAsync(cancellationToken);

                _logger.LogInformation("Successfully updated entity of type {EntityType} in {OperationName}",
                    typeof(T).Name, operationName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }
        #endregion

        #region Delete Operations
        /// <summary>
        /// Deletes an entity from the database
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            const string operationName = "DeleteAsync";

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _logger.LogDebug("Starting {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);

                dbSet.Remove(entity);
                await SaveAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted entity of type {EntityType} in {OperationName}",
                    typeof(T).Name, operationName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Deletes multiple entities from the database
        /// </summary>
        /// <param name="entities">Collection of entities to delete</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities is null or empty</exception>
        public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            const string operationName = "DeleteRangeAsync";

            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
            {
                _logger.LogWarning("Empty entities collection provided to {OperationName}", operationName);
                return;
            }

            try
            {
                _logger.LogDebug("Starting {OperationName} for {Count} entities of type {EntityType}",
                    operationName, entitiesList.Count, typeof(T).Name);

                dbSet.RemoveRange(entitiesList);
                await SaveAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted {Count} entities of type {EntityType} in {OperationName}",
                    entitiesList.Count, typeof(T).Name, operationName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled for entity type {EntityType}",
                    operationName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName} for {Count} entities of type {EntityType}",
                    operationName, entitiesList.Count, typeof(T).Name);
                throw;
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            const string operationName = "SaveAsync";

            try
            {
                _logger.LogDebug("Starting {OperationName} for database context", operationName);

                var changesCount = await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully saved {ChangesCount} changes to database in {OperationName}",
                    changesCount, operationName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation {OperationName} was cancelled", operationName);
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred in {OperationName}", operationName);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error occurred in {OperationName}", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName}", operationName);
                throw;
            }
        }
        #endregion
    }
    #endregion
}
