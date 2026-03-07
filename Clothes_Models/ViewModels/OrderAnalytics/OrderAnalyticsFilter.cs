using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class OrderAnalyticsFilter
    {
        public int Days { get; set; } = 30;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public string? Country { get; set; }
        public bool CompareWithPreviousPeriod { get; set; } = false;
    }
}
