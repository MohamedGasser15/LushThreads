using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.UserAnalytics
{
    public class UserAnalyticsViewModel
    {
        public UserGrowthViewModel UserGrowth { get; set; }
        public UserRolesDistributionViewModel RolesDistribution { get; set; }
        public UserStatusViewModel AccountStatus { get; set; }
        public RegistrationSourcesViewModel RegistrationSources { get; set; }
        public RecentActivityViewModel RecentActivity { get; set; }
        public int TotalUsers { get; set; }
    }
}
