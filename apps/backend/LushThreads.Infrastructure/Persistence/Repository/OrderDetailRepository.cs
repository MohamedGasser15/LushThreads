using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.OrderAnalytics;
using LushThreads.Domain.ViewModels.ProductAnalytics;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for OrderDetail entity.
    /// </summary>
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        public OrderDetailRepository(ApplicationDbContext db, ILogger<Repository<OrderDetail>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion

        #region Product Analytics Methods (Existing)

        public async Task<(string ProductName, int SalesCount, decimal Revenue)?> GetTopProductAsync(DateTime cutoffDate)
        {
            var result = await _db.OrderDetails
                .Include(od => od.OrderHeader)
                .Where(od => od.OrderHeader.OrderDate >= cutoffDate)
                .GroupBy(od => od.Product.Product_Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    SalesCount = g.Sum(od => od.Count),
                    Revenue = g.Sum(od => od.Count * od.price)
                })
                .OrderByDescending(x => x.SalesCount)
                .Select(x => new { x.ProductName, x.SalesCount, Revenue = (decimal)x.Revenue })
                .FirstOrDefaultAsync();

            if (result == null) return null;

            return (result.ProductName, result.SalesCount, result.Revenue);
        }

        public async Task<List<TopProductResult>> GetTopProductsAsync(DateTime cutoffDate, int count)
        {
            return await _db.OrderDetails
                .Include(od => od.OrderHeader)
                .Where(od => od.OrderHeader.OrderDate >= cutoffDate)
                .GroupBy(od => od.Product.Product_Name)
                .Select(g => new TopProductResult
                {
                    ProductName = g.Key,
                    SalesCount = g.Sum(od => od.Count),
                    Revenue = (decimal)g.Sum(od => od.Count * od.price)
                })
                .OrderByDescending(x => x.SalesCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<CategorySalesResult>> GetSalesByCategoryAsync(DateTime cutoffDate)
        {
            return await _db.OrderDetails
                .Include(od => od.OrderHeader)
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .Where(od => od.OrderHeader.OrderDate >= cutoffDate)
                .GroupBy(od => od.Product.Category.Category_Name)
                .Select(g => new CategorySalesResult
                {
                    CategoryName = g.Key,
                    SalesCount = g.Sum(od => od.Count),
                    Revenue = (decimal)g.Sum(od => od.Count * od.price)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
        }

        #endregion

        #region Order Analytics Methods

        /// <inheritdoc />
        public async Task<List<TopProductItem>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount, OrderAnalyticsFilter filter)
        {
            var query = _db.OrderDetails
                .Include(od => od.OrderHeader)
                .Include(od => od.Product)
                .Where(od => od.OrderHeader.OrderDate >= startDate && od.OrderHeader.OrderDate <= endDate);

            if (filter.ProductId.HasValue)
                query = query.Where(od => od.ProductId == filter.ProductId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(od => od.Product.Category_Id == filter.CategoryId.Value);

            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(od => od.OrderHeader.Country == filter.Country);

            var results = await query
                .GroupBy(od => new { od.ProductId, od.Product.Product_Name, od.Product.imgUrl })
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key.Product_Name,
                    ImageUrl = g.Key.imgUrl,
                    QuantitySold = g.Sum(od => od.Count),
                    Revenue = g.Sum(od => od.price * od.Count)
                })
                .OrderByDescending(g => g.Revenue)
                .Take(topCount)
                .ToListAsync();

            return results;
        }

        /// <inheritdoc />
        public async Task<List<CategorySalesItem>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = _db.OrderDetails
                .Include(od => od.OrderHeader)
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .Where(od => od.OrderHeader.OrderDate >= startDate && od.OrderHeader.OrderDate <= endDate);

            if (filter.CategoryId.HasValue)
                query = query.Where(od => od.Product.Category_Id == filter.CategoryId.Value);

            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(od => od.OrderHeader.Country == filter.Country);

            var results = await query
                .GroupBy(od => od.Product.Category.Category_Name)
                .Select(g => new CategorySalesItem
                {
                    CategoryName = g.Key,
                    OrderCount = g.Select(od => od.OrderHeaderId).Distinct().Count(),
                    Revenue = g.Sum(od => od.price * od.Count)
                })
                .OrderByDescending(g => g.Revenue)
                .ToListAsync();

            return results;
        }

        #endregion
    }
}