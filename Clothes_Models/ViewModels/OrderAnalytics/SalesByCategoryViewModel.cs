using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class SalesByCategoryViewModel
    {
        public List<string> CategoryNames { get; set; } = new();
        public List<int> OrderCounts { get; set; } = new();
        public List<decimal> Revenue { get; set; } = new();
        public List<string> Colors { get; set; } = new()
        {
            "#6366F1", "#10B981", "#3B82F6",
            "#F59E0B", "#EC4899", "#F97316"
        };
    }
}
