using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class CategorySalesItem
    {
        public string CategoryName { get; set; }
        public int OrderCount { get; set; }
        public double Revenue { get; set; }
    }
}
