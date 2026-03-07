using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class ProductAnalyticsViewModel
    {
        public KeyMetrics KeyMetrics { get; set; }
        public TopProductsData TopProducts { get; set; }
        public InventoryStatusData InventoryStatus { get; set; }
        public SalesByCategoryData SalesByCategory { get; set; }
        public List<LowStockItem> LowStockItems { get; set; }
    }
}
