using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class TopProductsData
    {
        public List<string> ProductNames { get; set; }
        public List<int> SalesCounts { get; set; }
        public List<decimal> RevenueGenerated { get; set; }
        public int TotalSales { get; set; }
    }
}
