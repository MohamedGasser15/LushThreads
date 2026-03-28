using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.OrderAnalytics;
using LushThreads.Domain.ViewModels.ProductAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Interface for OrderDetail repository.
    /// Inherits from the generic IRepository interface.
    /// </summary>
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        #region Product Analytics (Existing)

        Task<(string ProductName, int SalesCount, decimal Revenue)?> GetTopProductAsync(DateTime cutoffDate);
        Task<List<TopProductResult>> GetTopProductsAsync(DateTime cutoffDate, int count);
        Task<List<CategorySalesResult>> GetSalesByCategoryAsync(DateTime cutoffDate);

        #endregion

        #region Order Analytics Methods

        /// <summary>
        /// Gets top products by revenue for the specified period.
        /// </summary>
        Task<List<TopProductItem>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets sales data grouped by category.
        /// </summary>
        Task<List<CategorySalesItem>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        #endregion
    }
}
