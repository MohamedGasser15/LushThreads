using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.UserAnalytics
{
    public class RegistrationSourcesViewModel
    {
        public List<string> Sources { get; set; }
        public List<int> Counts { get; set; }
        public List<string> Colors { get; set; }
    }
}
