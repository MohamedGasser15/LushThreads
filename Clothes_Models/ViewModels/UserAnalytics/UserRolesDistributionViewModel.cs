using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.UserAnalytics
{
    public class UserRolesDistributionViewModel
    {
        public List<string> RoleNames { get; set; }
        public List<int> Counts { get; set; }
        public List<string> Colors { get; set; }
    }
}
