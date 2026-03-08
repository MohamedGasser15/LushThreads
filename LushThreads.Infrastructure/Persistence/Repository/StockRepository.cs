using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.ProductAnalytics;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
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

        public async Task<decimal> GetInventoryValueAsync()
        {
            return await _db.Stocks.SumAsync(s => s.Quantity * s.Product.Product_Price);
        }

        public async Task<int> GetLowStockCountAsync(int threshold)
        {
            return await _db.Stocks.CountAsync(s => s.Quantity <= threshold);
        }
        public async Task<int> GetLowStockCountAsync(int lowThreshold, int outOfStockThreshold)
        {
            return await _db.Stocks.CountAsync(s => s.Quantity <= lowThreshold && s.Quantity >= outOfStockThreshold);
        }
        public async Task<List<LowStockItem>> GetLowStockItemsAsync(int threshold)
        {
            return await _db.Stocks
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Include(s => s.Product.OrderDetails)
                    .ThenInclude(od => od.OrderHeader)
                .Where(s => s.Quantity <= threshold)
                .OrderBy(s => s.Quantity)
                .Select(s => new LowStockItem
                {
                    ProductName = s.Product.Product_Name,
                    Category = s.Product.Category.Category_Name,
                    Size = s.Size,
                    CurrentStock = s.Quantity,
                    Threshold = threshold,
                    LastSoldDate = s.Product.OrderDetails
                        .OrderByDescending(od => od.OrderHeader.OrderDate)
                        .Select(od => (DateTime?)od.OrderHeader.OrderDate)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }
        public async Task<int> GetInStockCountAsync(int lowStockThreshold)
        {
            return await _db.Stocks.CountAsync(s => s.Quantity > lowStockThreshold);
        }

        public async Task<int> GetOutOfStockCountAsync(int outOfStockThreshold)
        {
            return await _db.Stocks.CountAsync(s => s.Quantity < outOfStockThreshold);
        }
    }
}