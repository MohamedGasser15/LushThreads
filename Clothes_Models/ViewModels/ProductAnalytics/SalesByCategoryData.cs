using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class SalesByCategoryData
    {
        public List<string> CategoryNames { get; set; }
        public List<int> SalesCounts { get; set; }
        public List<decimal> Revenue { get; set; }
        public List<string> Colors { get; set; }
        public int TotalSales { get; set; }
    }
}
