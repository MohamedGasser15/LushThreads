using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class TopProductResult
    {
        public string ProductName { get; set; }
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
}