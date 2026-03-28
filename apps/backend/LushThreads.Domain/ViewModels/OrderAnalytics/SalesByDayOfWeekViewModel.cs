using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class SalesByDayOfWeekViewModel
    {
        public List<string> Days { get; set; } = new()
        {
            "Sunday", "Monday", "Tuesday", "Wednesday",
            "Thursday", "Friday", "Saturday"
        };
        public List<decimal> Revenue { get; set; } = new();
        public List<int> OrderCounts { get; set; } = new();
    }
}
