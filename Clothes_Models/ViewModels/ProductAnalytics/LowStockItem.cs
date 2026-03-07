using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class LowStockItem
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Size { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public DateTime? LastSoldDate { get; set; }
    }
}
