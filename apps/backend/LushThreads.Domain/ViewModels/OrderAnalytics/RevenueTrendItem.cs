using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class RevenueTrendItem
    {
        public DateTime Period { get; set; }
        public string Label { get; set; }
        public double Revenue { get; set; }
        public double Subtotal { get; set; }
        public double ShippingFees { get; set; }
        public double Taxes { get; set; }
        public int OrderCount { get; set; }
    }
}
