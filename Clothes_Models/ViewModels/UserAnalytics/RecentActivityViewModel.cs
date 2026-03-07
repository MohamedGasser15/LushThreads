using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.UserAnalytics
{
    public class RecentActivityViewModel
    {
        public int NewUsersToday { get; set; }
        public int LoginsLast24Hours { get; set; }
        public int PurchasesYesterday { get; set; }
    }
}
