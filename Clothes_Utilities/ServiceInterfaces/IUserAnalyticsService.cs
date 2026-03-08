using LushThreads.Domain.ViewModels.UserAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    public interface IUserAnalyticsService
    {
        Task<UserAnalyticsViewModel> GetUserAnalytics(int days);
        Task<UserGrowthViewModel> GetUserGrowthData(DateTime startDate, DateTime endDate, int days);
    }
}
