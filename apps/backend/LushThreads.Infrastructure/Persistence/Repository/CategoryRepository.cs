using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging; // Added for ILogger if needed, but not present in original

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for Category entity.
    /// Inherits from generic Repository and implements ICategoryRepository.
    /// </summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public CategoryRepository(ApplicationDbContext db, ILogger<Repository<Category>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion
    }
}