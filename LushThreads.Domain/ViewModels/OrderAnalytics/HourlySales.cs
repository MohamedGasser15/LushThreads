using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class HourlySales
    {
        public int Hour { get; set; }
        public double Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
