using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class KeyMetrics
    {
        public int TotalProducts { get; set; }
        public string TopSellingProduct { get; set; }
        public int TopProductSales { get; set; }
        public decimal InventoryValue { get; set; }
        public decimal InventoryChange { get; set; }
        public int LowStockItems { get; set; }
    }
}
