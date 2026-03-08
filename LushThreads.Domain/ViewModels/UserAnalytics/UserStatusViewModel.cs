using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.UserAnalytics
{
    public class UserStatusViewModel
    {
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
    }
}
