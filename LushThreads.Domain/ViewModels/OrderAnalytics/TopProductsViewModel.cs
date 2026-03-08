using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class TopProductsViewModel
    {
        public List<string> ProductNames { get; set; } = new();
        public List<int> QuantitiesSold { get; set; } = new();
        public List<decimal> RevenueGenerated { get; set; } = new();
        public List<string> ProductImages { get; set; } = new();
    }
}
