using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class TopProductItem
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public double Revenue { get; set; }
    }
}
