using LushThreads.Domain.ViewModels.ProductAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Provides methods for retrieving product analytics data.
    /// </summary>
    public interface IProductAnalyticsService
    {
        /// <summary>
        /// Retrieves product analytics data for the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look back for analytics.</param>
        /// <returns>A <see cref="ProductAnalyticsViewModel"/> containing all analytics data.</returns>
        Task<ProductAnalyticsViewModel> GetProductAnalytics(int days);
    }
}