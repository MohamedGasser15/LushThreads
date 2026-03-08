using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.OrderAnalytics
{
    public class OrderVolumeViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<int> CompletedOrders { get; set; } = new();
        public List<int> PendingOrders { get; set; } = new();
        public List<int> CancelledOrders { get; set; } = new();
    }
}
