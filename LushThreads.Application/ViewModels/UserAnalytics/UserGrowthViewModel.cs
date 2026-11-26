using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ViewModels.UserAnalytics
{
    public class UserGrowthViewModel
    {
        public List<string> Labels { get; set; }
        public List<int> NewUsers { get; set; }
        public List<int> TotalUsers { get; set; }
    }
}
