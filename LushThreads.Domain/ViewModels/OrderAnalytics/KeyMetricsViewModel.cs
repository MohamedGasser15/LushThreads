using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class KeyMetricsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public int TotalOrders { get; set; }
        public int OrderChangePercentage { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AOVChangePercentage { get; set; }
        public int NewCustomers { get; set; }
        public decimal RefundRate { get; set; }
        public Dictionary<string, decimal> ConversionRates { get; set; } = new()
        {
            ["CartToCheckout"] = 0m,
            ["CheckoutToPurchase"] = 0m
        };
    }
}
