using LushThreads.Domain.Entites;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task UpdateAsync(Category category);
    }
}
