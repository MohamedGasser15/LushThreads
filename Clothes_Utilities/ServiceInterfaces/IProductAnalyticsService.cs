using LushThreads.Domain.ViewModels.ProductAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    public interface IProductAnalyticsService
    {
        Task<ProductAnalyticsViewModel> GetProductAnalytics(int days);
    }
}
