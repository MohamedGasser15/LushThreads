using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.OrderAnalytics;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service responsible for providing order analytics data including revenue trends,
    /// order status distribution, top products, and other key metrics.
    /// </summary>
    public class OrderAnalyticsService : IOrderAnalyticsService
    {
        #region Fields

        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly ILogger<OrderAnalyticsService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderAnalyticsService"/> class.
        /// </summary>
        /// <param name="orderRepository">Repository for order operations.</param>
        /// <param name="orderDetailRepository">Repository for order detail operations.</param>
        /// <param name="logger">Logger instance.</param>
        public OrderAnalyticsService(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            ILogger<OrderAnalyticsService> logger)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<OrderAnalyticsViewModel> GetDashboardData(OrderAnalyticsFilter filter)
        {
            try
            {
                var endDate = filter.EndDate ?? DateTime.UtcNow;
                var startDate = filter.StartDate ?? endDate.AddDays(-filter.Days);

                _logger.LogInformation("Generating order analytics from {StartDate} to {EndDate}", startDate, endDate);

                return new OrderAnalyticsViewModel
                {
                    RevenueTrend = await GetRevenueTrend(startDate, endDate, filter),
                    OrderStatusDistribution = await GetOrderStatusDistribution(startDate, endDate, filter),
                    PaymentStatusDistribution = await GetPaymentStatusDistribution(startDate, endDate, filter),
                    TopProducts = await GetTopProducts(startDate, endDate, 5, filter),
                    SalesByDayOfWeek = await GetSalesByDayOfWeek(startDate, endDate, filter),
                    SalesByCategory = await GetSalesByCategory(startDate, endDate, filter),
                    SalesByHour = await GetSalesByHour(startDate, endDate, filter),
                    KeyMetrics = await GetKeyMetrics(startDate, endDate, filter),
                    TotalDays = (int)(endDate - startDate).TotalDays
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating order analytics");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RevenueTrendViewModel> GetRevenueTrendData(DateTime startDate, DateTime endDate, int days)
        {
            return await GetRevenueTrend(startDate, endDate, new OrderAnalyticsFilter
            {
                Days = days,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        /// <inheritdoc />
        public async Task<OrderStatusDistributionViewModel> GetOrderStatusData(DateTime startDate, DateTime endDate)
        {
            return await GetOrderStatusDistribution(startDate, endDate, new OrderAnalyticsFilter());
        }

        /// <inheritdoc />
        public async Task<TopProductsViewModel> GetTopProductsData(int topCount, DateTime startDate, DateTime endDate)
        {
            return await GetTopProducts(startDate, endDate, topCount, new OrderAnalyticsFilter());
        }

        #endregion

        #region Private Methods - Analytics Builders

        /// <summary>
        /// Builds revenue trend data.
        /// </summary>
        private async Task<RevenueTrendViewModel> GetRevenueTrend(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var interval = DetermineInterval(startDate, endDate);
            var trendItems = await _orderRepository.GetRevenueTrendAsync(startDate, endDate, filter, interval);

            var model = new RevenueTrendViewModel();
            foreach (var item in trendItems)
            {
                model.Labels.Add(item.Label);
                model.Revenue.Add((decimal)item.Revenue);
                model.Subtotal.Add((decimal)item.Subtotal);
                model.ShippingFees.Add((decimal)item.ShippingFees);
                model.Taxes.Add((decimal)item.Taxes);
                model.OrderCounts.Add(item.OrderCount);
            }

            return model;
        }

        /// <summary>
        /// Builds order status distribution.
        /// </summary>
        private async Task<OrderStatusDistributionViewModel> GetOrderStatusDistribution(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var statusGroups = await _orderRepository.GetOrderStatusDistributionAsync(startDate, endDate, filter);

            var model = new OrderStatusDistributionViewModel();
            var standardStatuses = new List<string> { "Delivered", "Pending", "Processing", "Cancelled", "Refunded", "Shipped", "Approved" };

            // Initialize with all standard statuses
            foreach (var status in standardStatuses)
            {
                model.Statuses.Add(status);
                var group = statusGroups.FirstOrDefault(g => string.Equals(g.Status, status, StringComparison.OrdinalIgnoreCase));
                model.Counts.Add(group?.Count ?? 0);
                model.RevenueByStatus.Add(group != null ? (decimal)group.Revenue : 0);
            }

            // Add any non-standard statuses
            foreach (var group in statusGroups.Where(g => !standardStatuses.Contains(g.Status, StringComparer.OrdinalIgnoreCase)))
            {
                model.Statuses.Add(group.Status);
                model.Counts.Add(group.Count);
                model.RevenueByStatus.Add((decimal)group.Revenue);
            }

            // Ensure colors match the number of statuses
            while (model.Colors.Count < model.Statuses.Count)
            {
                model.Colors.Add("#D1D5DB"); // Default gray
            }
            model.Colors = model.Colors.Take(model.Statuses.Count).ToList();

            return model;
        }

        /// <summary>
        /// Builds payment status distribution.
        /// </summary>
        private async Task<PaymentStatusDistributionViewModel> GetPaymentStatusDistribution(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var paymentGroups = await _orderRepository.GetPaymentStatusDistributionAsync(startDate, endDate, filter);

            var model = new PaymentStatusDistributionViewModel();
            foreach (var group in paymentGroups)
            {
                model.Statuses.Add(group.Status);
                model.Counts.Add(group.Count);
            }

            // Ensure colors match the number of statuses
            while (model.Colors.Count < model.Statuses.Count)
            {
                model.Colors.Add("#D1D5DB");
            }
            model.Colors = model.Colors.Take(model.Statuses.Count).ToList();

            return model;
        }

        /// <summary>
        /// Builds top products data.
        /// </summary>
        private async Task<TopProductsViewModel> GetTopProducts(DateTime startDate, DateTime endDate, int topCount, OrderAnalyticsFilter filter)
        {
            var topProducts = await _orderDetailRepository.GetTopProductsAsync(startDate, endDate, topCount, filter);

            var model = new TopProductsViewModel();
            foreach (var product in topProducts)
            {
                model.ProductNames.Add(product.ProductName);
                model.QuantitiesSold.Add(product.QuantitySold);
                model.RevenueGenerated.Add((decimal)product.Revenue);
                model.ProductImages.Add(product.ImageUrl ?? "/images/default-product.png");
            }

            return model;
        }

        /// <summary>
        /// Builds sales by day of week data.
        /// </summary>
        private async Task<SalesByDayOfWeekViewModel> GetSalesByDayOfWeek(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var dayGroups = await _orderRepository.GetSalesByDayOfWeekAsync(startDate, endDate, filter);

            var model = new SalesByDayOfWeekViewModel();
            // Initialize all days with zero
            for (int i = 0; i < 7; i++)
            {
                model.Revenue.Add(0);
                model.OrderCounts.Add(0);
            }

            foreach (var group in dayGroups)
            {
                int dayIndex = (int)group.DayOfWeek;
                model.Revenue[dayIndex] = (decimal)group.Revenue;
                model.OrderCounts[dayIndex] = group.OrderCount;
            }

            return model;
        }

        /// <summary>
        /// Builds sales by category data.
        /// </summary>
        private async Task<SalesByCategoryViewModel> GetSalesByCategory(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var categoryGroups = await _orderDetailRepository.GetSalesByCategoryAsync(startDate, endDate, filter);

            var model = new SalesByCategoryViewModel();
            foreach (var group in categoryGroups)
            {
                model.CategoryNames.Add(group.CategoryName);
                model.OrderCounts.Add(group.OrderCount);
                model.Revenue.Add((decimal)group.Revenue);
            }

            // Ensure colors match the number of categories
            while (model.Colors.Count < model.CategoryNames.Count)
            {
                model.Colors.Add("#D1D5DB");
            }
            model.Colors = model.Colors.Take(model.CategoryNames.Count).ToList();

            return model;
        }

        /// <summary>
        /// Builds sales by hour data.
        /// </summary>
        private async Task<SalesByHourViewModel> GetSalesByHour(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var hourGroups = await _orderRepository.GetSalesByHourAsync(startDate, endDate, filter);

            var model = new SalesByHourViewModel();
            // Initialize all hours with zero
            for (int i = 0; i < 24; i++)
            {
                model.Revenue.Add(0);
                model.OrderCounts.Add(0);
            }

            foreach (var group in hourGroups)
            {
                model.Revenue[group.Hour] = (decimal)group.Revenue;
                model.OrderCounts[group.Hour] = group.OrderCount;
            }

            return model;
        }

        /// <summary>
        /// Builds key metrics data.
        /// </summary>
        private async Task<KeyMetricsViewModel> GetKeyMetrics(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var (currentOrders, previousOrders) = await _orderRepository.GetOrdersForMetricsAsync(startDate, endDate, filter);

            var currentRevenue = currentOrders.Sum(o => o.OrderTotal);
            var previousRevenue = previousOrders.Sum(o => o.OrderTotal);
            var revenueChange = previousRevenue > 0
                ? (currentRevenue - previousRevenue) / previousRevenue
                : 0;

            var currentOrderCount = currentOrders.Count;
            var previousOrderCount = previousOrders.Count;
            var orderChangePercentage = previousOrderCount > 0
                ? (currentOrderCount - previousOrderCount) / (double)previousOrderCount
                : 0;

            var averageOrderValue = currentOrderCount > 0 ? currentRevenue / currentOrderCount : 0;
            var previousAOV = previousOrderCount > 0 ? previousRevenue / previousOrderCount : 0;
            var aovChange = previousAOV > 0 ? (averageOrderValue - previousAOV) / previousAOV : 0;

            var newCustomers = await _orderRepository.GetNewCustomerCountAsync(startDate, endDate, filter);

            // Refund rate calculation (assuming orders with status "Refunded")
            var refundedRevenue = currentOrders.Where(o => o.OrderStatus == "Refunded").Sum(o => o.OrderTotal);
            var refundRate = currentRevenue > 0 ? refundedRevenue / currentRevenue : 0;

            return new KeyMetricsViewModel
            {
                TotalRevenue = (decimal)currentRevenue,
                RevenueChangePercentage = (decimal)revenueChange,
                TotalOrders = currentOrderCount,
                OrderChangePercentage = (int)(orderChangePercentage * 100),
                AverageOrderValue = (decimal)averageOrderValue,
                AOVChangePercentage = (decimal)aovChange,
                NewCustomers = newCustomers,
                RefundRate = (decimal)refundRate,
                ConversionRates = new Dictionary<string, decimal>
                {
                    ["CartToCheckout"] = 0m, // Placeholder - would need additional data
                    ["CheckoutToPurchase"] = 0m
                }
            };
        }

        /// <summary>
        /// Determines the interval for grouping based on date range length.
        /// </summary>
        private string DetermineInterval(DateTime startDate, DateTime endDate)
        {
            var days = (endDate - startDate).TotalDays;
            return days <= 7 ? "day" : days <= 30 ? "week" : "month";
        }

        #endregion
    }
}