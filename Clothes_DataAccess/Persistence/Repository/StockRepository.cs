using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for Stock entity.
    /// Inherits from generic Repository and implements IStockRepository.
    /// </summary>
    public class StockRepository : Repository<Stock>, IStockRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StockRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="logger">Logger for the generic repository operations.</param>
        public StockRepository(ApplicationDbContext db, ILogger<Repository<Stock>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion
    }
}