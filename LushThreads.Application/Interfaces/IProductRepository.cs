using LushThreads.Domain.Entites;

namespace LushThreads.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<int> GetTotalProductsCountAsync();
    }
}
