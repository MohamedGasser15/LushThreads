using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class RevenueTrendViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Revenue { get; set; } = new();
        public List<decimal> Subtotal { get; set; } = new();
        public List<decimal> ShippingFees { get; set; } = new();
        public List<decimal> Taxes { get; set; } = new();
        public List<int> OrderCounts { get; set; } = new();
    }
}
