using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for Product entity.
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRepository"/> class.
        /// </summary>
        /// <param name="db">Application database context.</param>
        /// <param name="logger">Logger instance.</param>
        public ProductRepository(ApplicationDbContext db, ILogger<Repository<Product>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion
    }
}
