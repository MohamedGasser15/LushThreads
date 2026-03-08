using LushThreads.Domain.ViewModels.OrderAnalytics;
using System;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Provides methods for retrieving order analytics data.
    /// </summary>
    public interface IOrderAnalyticsService
    {
        /// <summary>
        /// Retrieves dashboard data for order analytics based on filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria for the analytics.</param>
        /// <returns>A <see cref="OrderAnalyticsViewModel"/> containing all order analytics data.</returns>
        Task<OrderAnalyticsViewModel> GetDashboardData(OrderAnalyticsFilter filter);

        /// <summary>
        /// Retrieves revenue trend data for the specified date range.
        /// </summary>
        Task<RevenueTrendViewModel> GetRevenueTrendData(DateTime startDate, DateTime endDate, int days);

        /// <summary>
        /// Retrieves order status distribution data for the specified date range.
        /// </summary>
        Task<OrderStatusDistributionViewModel> GetOrderStatusData(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves top products data for the specified date range.
        /// </summary>
        Task<TopProductsViewModel> GetTopProductsData(int topCount, DateTime startDate, DateTime endDate);
    }
}