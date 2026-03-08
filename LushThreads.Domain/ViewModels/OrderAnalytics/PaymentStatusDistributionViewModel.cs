using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class PaymentStatusDistributionViewModel
    {
        public List<string> Statuses { get; set; } = new();
        public List<int> Counts { get; set; } = new();
        public List<string> Colors { get; set; } = new()
        {
            "#10B981", // Paid
            "#3B82F6", // Pending
            "#EF4444"  // Refunded
        };
    }
}
