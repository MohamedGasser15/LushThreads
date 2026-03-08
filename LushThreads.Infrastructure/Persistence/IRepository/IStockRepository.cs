using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.ProductAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<decimal> GetInventoryValueAsync();
        Task<int> GetLowStockCountAsync(int threshold);
        Task<List<LowStockItem>> GetLowStockItemsAsync(int threshold);
        Task<int> GetInStockCountAsync(int lowStockThreshold);
        Task<int> GetLowStockCountAsync(int lowThreshold, int outOfStockThreshold);
        Task<int> GetOutOfStockCountAsync(int outOfStockThreshold);
    }
}
