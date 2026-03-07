using LushThreads.Domain.Entites;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
       Task UpdateAsync (Product product);
    }
}
