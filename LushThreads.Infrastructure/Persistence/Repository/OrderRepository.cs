using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.OrderAnalytics;
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
    /// Repository implementation for OrderHeader entity.
    /// </summary>
    public class OrderRepository : Repository<OrderHeader>, IOrderRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        public OrderRepository(ApplicationDbContext db, ILogger<Repository<OrderHeader>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion

        #region Existing Methods

        public async Task<OrderHeader?> GetOrderWithUserAsync(int orderId)
        {
            return await _db.OrderHeaders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersWithUserAsync()
        {
            return await _db.OrderHeaders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            return await _db.OrderDetails
                .Where(od => od.OrderHeaderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductAsync(int orderId)
        {
            return await _db.OrderDetails
                .Where(od => od.OrderHeaderId == orderId)
                .Include(od => od.Product)
                .ToListAsync();
        }

        public async Task UpdateStockForOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails, bool increase)
        {
            foreach (var detail in orderDetails)
            {
                var stock = await _db.Stocks
                    .FirstOrDefaultAsync(s => s.Product_Id == detail.ProductId && s.Size == detail.Size);

                if (stock == null)
                    throw new InvalidOperationException($"Stock not found for Product ID {detail.ProductId} with size {detail.Size}.");

                if (increase)
                    stock.Quantity += detail.Count;
                else
                {
                    if (stock.Quantity < detail.Count)
                        throw new InvalidOperationException($"Insufficient stock for Product ID {detail.ProductId} (Size: {detail.Size}). Available: {stock.Quantity}, Requested: {detail.Count}.");
                    stock.Quantity -= detail.Count;
                }

                _db.Stocks.Update(stock);
            }
        }

        #endregion

        #region Analytics Methods

        /// <inheritdoc />
        public async Task<List<RevenueTrendItem>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter, string interval)
        {
            var query = BuildFilteredQuery(startDate, endDate, filter);

            var results = new List<RevenueTrendItem>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var (nextDate, label) = GetNextInterval(currentDate, interval);

                var ordersInPeriod = await query
                    .Where(o => o.OrderDate >= currentDate && o.OrderDate < nextDate)
                    .Select(o => new
                    {
                        o.OrderTotal,
                        o.Subtotal,
                        o.ShippingFee,
                        o.Tax
                    })
                    .ToListAsync();

                results.Add(new RevenueTrendItem
                {
                    Period = currentDate,
                    Label = label,
                    Revenue = ordersInPeriod.Sum(o => o.OrderTotal),
                    Subtotal = ordersInPeriod.Sum(o => o.Subtotal),
                    ShippingFees = ordersInPeriod.Sum(o => o.ShippingFee),
                    Taxes = ordersInPeriod.Sum(o => o.Tax),
                    OrderCount = ordersInPeriod.Count
                });

                currentDate = nextDate;
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<List<OrderStatusGroup>> GetOrderStatusDistributionAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = BuildFilteredQuery(startDate, endDate, filter);

            return await query
                .GroupBy(o => o.OrderStatus ?? "Unknown")
                .Select(g => new OrderStatusGroup
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.OrderTotal)
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<PaymentStatusGroup>> GetPaymentStatusDistributionAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = BuildFilteredQuery(startDate, endDate, filter);

            return await query
                .GroupBy(o => o.PaymentStatus ?? "Unknown")
                .Select(g => new PaymentStatusGroup
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<(List<OrderHeader> currentOrders, List<OrderHeader> previousOrders)> GetOrdersForMetricsAsync(
            DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var periodLength = (endDate - startDate).TotalDays;
            var previousStartDate = startDate.AddDays(-periodLength);
            var previousEndDate = startDate;

            var currentQuery = BuildFilteredQuery(startDate, endDate, filter);
            var previousQuery = BuildFilteredQuery(previousStartDate, previousEndDate, filter);

            var currentOrders = await currentQuery.ToListAsync();
            var previousOrders = await previousQuery.ToListAsync();

            return (currentOrders, previousOrders);
        }

        /// <inheritdoc />
        public async Task<int> GetNewCustomerCountAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = _db.OrderHeaders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .GroupBy(o => o.ApplicationUserId)
                .Where(g => g.Min(o => o.OrderDate) >= startDate)
                .Select(g => g.Key);

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(userId => _db.OrderHeaders
                    .Any(o => o.ApplicationUserId == userId && o.Country == filter.Country));
            }

            return await query.CountAsync();
        }

        /// <inheritdoc />
        public async Task<List<DayOfWeekSales>> GetSalesByDayOfWeekAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = BuildFilteredQuery(startDate, endDate, filter);

            var results = await query
                .GroupBy(o => o.OrderDate.DayOfWeek)
                .Select(g => new DayOfWeekSales
                {
                    DayOfWeek = g.Key,
                    Revenue = g.Sum(o => o.OrderTotal),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            return results;
        }

        /// <inheritdoc />
        public async Task<List<HourlySales>> GetSalesByHourAsync(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = BuildFilteredQuery(startDate, endDate, filter);

            var results = await query
                .GroupBy(o => o.OrderDate.Hour)
                .Select(g => new HourlySales
                {
                    Hour = g.Key,
                    Revenue = g.Sum(o => o.OrderTotal),
                    OrderCount = g.Count()
                })
                .OrderBy(g => g.Hour)
                .ToListAsync();

            return results;
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<OrderHeader> BuildFilteredQuery(DateTime startDate, DateTime endDate, OrderAnalyticsFilter filter)
        {
            var query = _db.OrderHeaders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.OrderStatus == filter.OrderStatus);

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);

            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(o => o.Country == filter.Country);

            return query;
        }

        private (DateTime nextDate, string label) GetNextInterval(DateTime currentDate, string interval)
        {
            return interval switch
            {
                "day" => (currentDate.AddDays(1), currentDate.ToString("MMM dd")),
                "week" => (currentDate.AddDays(7), $"Week {GetWeekOfMonth(currentDate)}"),
                _ => (currentDate.AddMonths(1), currentDate.ToString("MMM yyyy"))
            };
        }

        private int GetWeekOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime firstDayOfMonth = new(date.Year, date.Month, 1);
            DateTime firstMonday = firstDayOfMonth.AddDays((DayOfWeek.Monday + 7 - firstDayOfMonth.DayOfWeek) % 7);
            if (firstMonday > date)
            {
                firstDayOfMonth = firstDayOfMonth.AddMonths(-1);
                firstMonday = firstDayOfMonth.AddDays((DayOfWeek.Monday + 7 - firstDayOfMonth.DayOfWeek) % 7);
            }
            return (date - firstMonday).Days / 7 + 1;
        }

        #endregion
    }
}