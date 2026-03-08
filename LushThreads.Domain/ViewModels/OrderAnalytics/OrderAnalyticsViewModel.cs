using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class OrderAnalyticsViewModel
    {
        public RevenueTrendViewModel RevenueTrend { get; set; } = new();
        public OrderVolumeViewModel OrderVolume { get; set; } = new();
        public OrderStatusDistributionViewModel OrderStatusDistribution { get; set; } = new();
        public PaymentStatusDistributionViewModel PaymentStatusDistribution { get; set; } = new();
        public TopProductsViewModel TopProducts { get; set; } = new();
        public SalesByDayOfWeekViewModel SalesByDayOfWeek { get; set; } = new();
        public SalesByCategoryViewModel SalesByCategory { get; set; } = new();
        public SalesByHourViewModel SalesByHour { get; set; } = new();
        public KeyMetricsViewModel KeyMetrics { get; set; } = new();
        public int TotalDays { get; set; } = 30;
    }
}
