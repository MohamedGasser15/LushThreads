using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class OrderStatusDistributionViewModel
    {
        public List<string> Statuses { get; set; } = new();
        public List<int> Counts { get; set; } = new();
        public List<string> Colors { get; set; } = new()
        {
        "#10B981", // Delivered - Emerald (success)
        "#F59E0B", // Pending - Blue (awaiting action)
        "#3B82F6", // Processing - Amber (in progress)
        "#EF4444", // Cancelled - Red (error/failure)
        "#8B5CF6", // Refunded - Purple (money returned)
        "#06B6D4", // Shipped - Cyan (in transit)
        "#84CC16", // Approved - Lime (verified)
        };
        public List<decimal> RevenueByStatus { get; set; } = new();
    }
}
