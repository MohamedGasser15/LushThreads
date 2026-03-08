using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.OrderAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for Order-specific operations.
    /// Inherits from generic IRepository for basic CRUD.
    /// </summary>
    public interface IOrderRepository : IRepository<OrderHeader>
    {
        /// <summary>
        /// Gets an order with its associated user.
        /// </summary>
        Task<OrderHeader?> GetOrderWithUserAsync(int orderId);

        /// <summary>
        /// Gets all orders with user information, ordered by newest first.
        /// </summary>
        Task<IEnumerable<OrderHeader>> GetAllOrdersWithUserAsync();

        /// <summary>
        /// Gets order details for a specific order.
        /// </summary>
        Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId);

        /// <summary>
        /// Gets order details with product information.
        /// </summary>
        Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductAsync(int orderId);

        /// <summary>
        /// Updates stock quantities based on order details.
        /// </summary>
        Task UpdateStockForOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails, bool increase);

        #region Analytics Methods

        /// <summary>
        /// Gets revenue trend data grouped by the specified interval.
        /// </summary>
        Task<List<RevenueTrendItem>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter, string interval);

        /// <summary>
        /// Gets order status distribution data.
        /// </summary>
        Task<List<OrderStatusGroup>> GetOrderStatusDistributionAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets payment status distribution data.
        /// </summary>
        Task<List<PaymentStatusGroup>> GetPaymentStatusDistributionAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets key metrics for the specified period and the previous period.
        /// </summary>
        Task<(List<OrderHeader> currentOrders, List<OrderHeader> previousOrders)> GetOrdersForMetricsAsync(
            DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets the count of new customers in the period.
        /// </summary>
        Task<int> GetNewCustomerCountAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets sales data grouped by day of week.
        /// </summary>
        Task<List<DayOfWeekSales>> GetSalesByDayOfWeekAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        /// <summary>
        /// Gets sales data grouped by hour of day.
        /// </summary>
        Task<List<HourlySales>> GetSalesByHourAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter);

        #endregion
    }
}
