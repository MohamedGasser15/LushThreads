using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class OrderStatusGroup
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public double Revenue { get; set; }
    }
}
