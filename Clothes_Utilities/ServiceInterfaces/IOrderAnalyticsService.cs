using LushThreads.Domain.ViewModels.OrderAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    public interface IOrderAnalyticsService
    {
        Task<OrderAnalyticsViewModel> GetDashboardData(OrderAnalyticsFilter filter);
        Task<RevenueTrendViewModel> GetRevenueTrendData(DateTime startDate, DateTime endDate, int days);
        Task<OrderStatusDistributionViewModel> GetOrderStatusData(DateTime startDate, DateTime endDate);
        Task<TopProductsViewModel> GetTopProductsData(int topCount, DateTime startDate, DateTime endDate);
    }
}
