using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.ProductAnalytics
{
    public class InventoryStatusData
    {
        public List<string> Statuses { get; set; }
        public List<int> Counts { get; set; }
        public List<string> Colors { get; set; }
        public int TotalItems { get; set; }
    }
}
